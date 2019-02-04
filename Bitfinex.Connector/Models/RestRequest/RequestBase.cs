using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Bitfinex.Connector.Infrastructure;
using Newtonsoft.Json;
using RestSharp;

namespace Bitfinex.Connector.Models.RestRequest
{
    /// <summary>
    /// Базовая функциональность классов запросов к серверу.
    /// </summary>
    /// <typeparam name="T">Модель данных, на которую должны проецироваться полученные от сервера данные.</typeparam>
    internal abstract class RequestBase<T>
    {
        /// <summary>
        /// Кэш параметров запроса. Используется для предотвращения использования рефлексии при выполнении каждого запроса.
        /// </summary>
        private static readonly ConcurrentBag<QueryParametersCasheItem> _queryParametersCashe = new ConcurrentBag<QueryParametersCasheItem>();

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
        protected RestSharp.RestRequest _request;

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
            InitQueryParametersCashe();

            _request = new RestSharp.RestRequest();
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
        /// Добавляет непустые значения параметров запроса к самому HTTP-запроса.
        /// </summary>
        private void AddQueryParameters()
        {
            foreach (QueryParametersCasheItem item in _queryParametersCashe)
            {
                AddQueryParameter(item);
            }
        }

        /// <summary>
        /// Добавляет новый параметр (строка запроса, путь) HTTP-запроса. Если значение параметра пустое - параметр не добавляется.
        /// </summary>
        /// <param name="casheItem">Элемент кэша параметров запроса, описывающий конкретный параметр.</param>
        private void AddQueryParameter(QueryParametersCasheItem casheItem)
        {
            object value = casheItem.PropertyInfo.GetValue(this);
            if (value == null)
            {
                return;
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
                switch (casheItem.ParameterType)
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

                _request.AddParameter(casheItem.Name.ToLowerInvariant(), ValueToQueryParameter(value), requestParameterType);
            }
        }

        /// <summary>
        /// Выполняет поиск всех свойств класса с атрибутом <see cref="QueryParameterAttribute"/> и заполняет кэш параметров запроса соответствующими данными.
        /// </summary>
        private void InitQueryParametersCashe()
        {
            if (_queryParametersCashe.Count > 0)
            {
                return;
            }

            Type requestType = GetType();

            var casheItems =
                from propertyInfo in requestType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                let attributesArray = propertyInfo.GetCustomAttributes(typeof(QueryParameterAttribute), true)
                where attributesArray.Length == 1
                let attribute = attributesArray[0] as QueryParameterAttribute
                select new QueryParametersCasheItem
                {
                    Name = string.IsNullOrWhiteSpace(attribute.Name) ? propertyInfo.Name : attribute.Name,
                    ParameterType = attribute.ParameterType,
                    PropertyInfo = propertyInfo
                };

            foreach (QueryParametersCasheItem item in casheItems)
            {
                _queryParametersCashe.Add(item);
            }
        }
    }
}
