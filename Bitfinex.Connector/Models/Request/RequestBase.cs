using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Bitfinex.Connector.Infrastructure;
using Newtonsoft.Json;
using RestSharp;

namespace Bitfinex.Connector.Models.Request
{
    /// <summary>
    /// Базовая функциональность классов запросов к серверу.
    /// </summary>
    /// <typeparam name="T">Модель данных, на которую должны проецироваться полученные от сервера данные.</typeparam>
    internal abstract class RequestBase<T>
    {
        /// <summary>
        /// Имя сегмента конечно точки запроса.
        /// </summary>
        private const string _endpointSegmentName = "endpoint";

        /// <summary>
        /// Базовый URL для доступа к личному кабинету.
        /// </summary>
        private const string _baseUrl = "https://api.bitfinex.com";

        /// <summary>
        /// Префикс ресурса сервера, используемый как шаблон для формирования действительного адреса ресурса.
        /// </summary>
        private readonly string _resourcePrefix = $"v2/{{{_endpointSegmentName}}}/";

        /// <summary>
        /// Запрос, который будет отправлен на сервер для выполнения требуемой операции с данными.
        /// </summary>
        protected RestRequest _request;

        /// <summary>
        /// Имя конкретной конечной точки, к которой должен быть выполнен запрос. Используется для формирования адреса ресурса сервера. Например: "trades".
        /// </summary>
        protected abstract string EndpointName { get; }

        /// <summary>
        /// Крайняя правая часть адреса ресурса, следующая сразу после общей части адреса, соответствующей общему шаблону префикса адреса. Преимущественно используется для указания требуемых частей маршрутизации адреса ресурса.
        /// </summary>
        protected abstract string ResourceSuffix { get; }

        /// <summary>
        /// Основной конструктор.
        /// </summary>
        protected RequestBase()
        {
            _request = new RestRequest();
        }

        /// <summary>
        /// Выполнение подготовленного запроса к серверу.
        /// </summary>
        /// <returns>Данные, определяемые моделью данных.</returns>
        public abstract Task<T> ExecuteAsync();

        /// <summary>
        /// Базовая реализация выполнения запроса. Получает данные с сервера и вызывает реализацию метода в конкретном классе для конвертации полученных данных в требуемый тип.
        /// </summary>
        /// <typeparam name="TData">Тип, к которому буду приведены сырые данные, полученные с сервера. В дальнейшем эти данные будут преобразованы в тип <see cref="T"/>.</typeparam>
        /// <param name="converter">Метод преобразования сырых данных, полученных с сервера, в требуемый тип <see cref="T"/>.</param>
        /// <returns>Данные, определяемые моделью данных.</returns>
        protected async Task<T> ExecuteAsync<TData>(Func<TData, T> converter) where TData : class, new()
        {
            TData data = await GetDataAsync<TData>().ConfigureAwait(false);
            if (data == null)
            {
                return default;
            }

            return converter(data);
        }

        /// <summary>
        /// Выполняет запрос и получает сырые данные с сервера.
        /// </summary>
        /// <typeparam name="TResult">Тип, к которому буду приведены данные, полученные с сервера.</typeparam>
        /// <returns>Данные, полученные непосредственно с сервера и приведённые к типу <see cref="TResult"/>.</returns>
        protected async Task<TResult> GetDataAsync<TResult>() where TResult : class, new()
        {
            _request.Resource = string.Concat(_resourcePrefix, ResourceSuffix.ToLowerInvariant());
            _request.AddUrlSegment(_endpointSegmentName, EndpointName.ToLowerInvariant());

            AddQueryParameters();

            RestClient client = new RestClient(_baseUrl);
            IRestResponse<TResult> response = await client.ExecuteTaskAsync<TResult>(_request).ConfigureAwait(false);

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    if (!response.IsSuccessful
                        && response.ErrorException != null)
                    {
                        throw response.ErrorException;
                    }
                    return response.Data;

                default:
                    string errorMessage = $"Status code: {(int)response.StatusCode} ({response.StatusCode}); Status description: {response.StatusDescription}";

                    switch (response.ContentType)
                    {
                        case "application/json":
                            var definition = new { message = "" };
                            var content = JsonConvert.DeserializeAnonymousType(response.Content, definition);

                            errorMessage += $"\n{content.message}";

                            break;

                        case "text/html":
                            errorMessage += $"\n{response.Content}";

                            break;
                    }

                    throw new ApplicationException(errorMessage);
            }
        }

        /// <summary>
        /// Преобразует переданное значение в строковое представление, пригодное для использования в качестве параметра HTTP-запроса.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <returns>Строковое представление параметра запроса.</returns>
        protected string ValueToQueryParameter(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string queryStringValue = string.Empty;

            //Type valueType = value.GetType();
            //if (valueType == typeof(DateTime))
            //{
            //    queryStringValue = ((DateTime)value).ToQueryString();
            //}
            //else if (valueType.IsEnum)
            //{
            //    queryStringValue = ConvertHelper.PascalCaseToSnakeCase(value);
            //}
            //else
            //{
            queryStringValue = value.ToString();
            //}

            return queryStringValue;
        }

        /// <summary>
        /// Выполняет поиск всех свойств класса с атрибутом <see cref="QueryParameterAttribute"/> и добавляет непустые значения этих свойств к параметрам HTTP-запроса.
        /// </summary>
        private void AddQueryParameters()
        {
            // TODO: Здесь можно закешировать получение набора свойств для каждого конкретного типа запросов.
            var parameters =
                from propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                let attributesArray = propertyInfo.GetCustomAttributes(typeof(QueryParameterAttribute), true)
                where attributesArray.Length == 1
                let attribute = attributesArray[0] as QueryParameterAttribute
                select new
                {
                    Name = string.IsNullOrWhiteSpace(attribute.Name) ? propertyInfo.Name : attribute.Name,
                    attribute.ParameterType,
                    Value = propertyInfo.GetValue(this)
                };
            foreach (var parameter in parameters)
            {
                AddQueryParameter(parameter.ParameterType, parameter.Name, parameter.Value);
            }
        }

        /// <summary>
        /// Добавляет новый параметр (строка запроса, путь) HTTP-запроса. Если значение параметра пустое - параметр не добавляется.
        /// </summary>
        /// <param name="parameterType">Тип параметра.</param>
        /// <param name="name">Имя параметра.</param>
        /// <param name="value">Значение параметра.</param>
        private void AddQueryParameter(QueryParameterType parameterType, string name, object value)
        {
            if (value == null)
            {
                return;
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            object GetDefaultValue(Type type)
            {
                if (type.IsEnum)
                {
                    return null;
                }
                else if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
                else
                {
                    return null;
                }
            };

            if (!value.Equals(GetDefaultValue(value.GetType())))
            {
                ParameterType requestParameterType;
                switch (parameterType)
                {
                    case QueryParameterType.QueryString:
                        requestParameterType = ParameterType.QueryString;
                        break;
                    case QueryParameterType.UrlSegment:
                        requestParameterType = ParameterType.UrlSegment;
                        break;
                    default:
                        throw new ApplicationException("Неизвестный тип параметров запроса.");
                }

                _request.AddParameter(name.ToLowerInvariant(), ValueToQueryParameter(value), requestParameterType);
            }
        }
    }
}
