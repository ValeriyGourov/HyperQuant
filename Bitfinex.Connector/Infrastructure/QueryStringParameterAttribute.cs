using System;

namespace Bitfinex.Connector.Infrastructure
{
    /// <summary>
    /// Определяет свойства класса, которые будут использованы для формирования параметров строки HTTP-запроса.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    internal class QueryStringParameterAttribute : Attribute
    {
        /// <summary>
        /// Имя параметра, как оно должно быть указано в строке запроса.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Конструктор по умолчанию, принимающий имя параметра строки запроса.
        /// </summary>
        /// <param name="name">Имя параметра.</param>
        public QueryStringParameterAttribute(string name) => Name = name;
    }
}
