using System.Reflection;

namespace Bitfinex.Connector.Infrastructure
{
    /// <summary>
    /// Элемент кеша параметров запроса, используемый для предотвращения использования рефлексии при каждом выполнении запросов.
    /// </summary>
    internal class QueryParametersCasheItem
    {
        /// <summary>
        /// Имя параметра запроса.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Тип параметра запроса.
        /// </summary>
        public QueryParameterType ParameterType { get; set; }

        /// <summary>
        /// Метаданные атрибута свойства запроса, описывающего параметр запроса.
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }
}
