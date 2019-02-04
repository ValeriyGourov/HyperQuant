namespace Bitfinex.Connector.Infrastructure
{
    /// <summary>
    /// Типы параметров HTTP-запроса.
    /// </summary>
    internal enum QueryParameterType
    {
        /// <summary>
        /// Строка запроса.
        /// </summary>
        QueryString,

        /// <summary>
        /// Сегмент пути запроса.
        /// </summary>
        UrlSegment
    }
}
