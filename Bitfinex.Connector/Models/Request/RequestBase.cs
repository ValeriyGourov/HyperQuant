using System;
using System.Linq;
using System.Net;
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
        /// Базовый URL для доступа к личному кабинету.
        /// </summary>
        private const string _baseUrl = "https://api.bitfinex.com";

        /// <summary>
        /// Префикс ресурса сервера, используемый как шаблон для формирования действительного адреса ресурса.
        /// </summary>
        private const string _resourcePrefix = "v2/{endpoint}/";

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

        protected async Task<TResult> GetDataAsync<TResult>() where TResult : class, new()
        {
            _request.Resource = string.Concat(_resourcePrefix, ResourceSuffix);
            _request.AddUrlSegment("endpoint", EndpointName);

            AddQueryStringParameters();

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
        /// Преобразует переданное значение в строковое представление, пригодное для использования в качестве параметра строки HTTP-запроса.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <returns>Строковое представление параметра строки запроса.</returns>
        protected string ValueToQueryString(object value)
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
        /// Выполняет поиск всех свойств класса с атрибутом QueryStringParameterAttribute и добавляет непустые значения этих свойств к строке HTTP-запроса.
        /// </summary>
        private void AddQueryStringParameters()
        {
            // TODO: Здесь можно закешировать получение набора свойств для каждого конкретного типа запросов.
            var parameters =
                from propertyInfo in GetType().GetProperties()
                let attributesArray = propertyInfo.GetCustomAttributes(typeof(QueryStringParameterAttribute), true)
                where attributesArray.Length == 1
                select new
                {
                    (attributesArray[0] as QueryStringParameterAttribute).Name,
                    Value = propertyInfo.GetValue(this)
                };
            foreach (var parameter in parameters)
            {
                AddQueryStringParameter(parameter.Name, parameter.Value);
            }
        }

        /// <summary>
        /// Добавляет к строке HTTP-запроса новый параметр. Если значение параметра пустое - параметр не добавляется.
        /// </summary>
        /// <param name="name">Имя параметра.</param>
        /// <param name="value">Значение параметра.</param>
        private void AddQueryStringParameter(string name, object value)
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
                _request.AddParameter(name, ValueToQueryString(value), ParameterType.QueryString);
            }
        }
    }
}
