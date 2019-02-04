using System;

namespace Bitfinex.Connector.Infrastructure
{
    /// <summary>
    /// Определяет свойства класса, которые будут использованы для формирования параметров HTTP-запроса.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class QueryParameterAttribute : Attribute
    {
        /// <summary>
        /// Тип параметра запроса.
        /// </summary>
        public QueryParameterType ParameterType { get; set; }

        /// <summary>
        /// Имя параметра, как оно должно быть указано в строке запроса. Имя параметра запроса не чувствительно к регистру символов. Если имя не указано, будет использовано имя свойства, к которому применён атрибут.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Конструктор по умолчанию, принимающий имя параметра строки запроса.
        /// </summary>
        /// <param name="parameterType">Тип параметра.</param>
        public QueryParameterAttribute(QueryParameterType parameterType) => ParameterType = parameterType;
    }
}
