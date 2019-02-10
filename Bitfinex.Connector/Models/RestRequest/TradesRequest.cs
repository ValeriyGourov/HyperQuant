using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bitfinex.Connector.Converters;
using Bitfinex.Connector.Infrastructure;
using Bitfinex.Domain;

[assembly: InternalsVisibleTo("Bitfinex.Connector.Tests")]

namespace Bitfinex.Connector.Models.RestRequest
{
    internal class TradesRequest : RequestBase<IEnumerable<Trade>>
    {
        private readonly TradeConverter _tradesConverter;

        #region Параметры строки запроса

        [QueryParameter(QueryParameterType.QueryString)]
        public int Limit { get; set; }

        [QueryParameter(QueryParameterType.QueryString)]
        public int Start { get; set; }

        [QueryParameter(QueryParameterType.QueryString)]
        public int End { get; set; }

        [QueryParameter(QueryParameterType.QueryString)]
        public int Sort { get; set; }

        #endregion

        #region Параметры пути запроса

        [QueryParameter(QueryParameterType.UrlSegment)]
        private Symbol Symbol { get; }

        #endregion

        protected override sealed string EndpointName => "trades";

        protected override sealed string ResourceSuffix => "{symbol}/hist";

        public TradesRequest(string symbol)
        {
            Symbol = new Symbol(symbol ?? throw new ArgumentNullException(nameof(symbol)));
            _tradesConverter = new TradeConverter(Symbol);
        }

        public override Task<IEnumerable<Trade>> ExecuteAsync() => ExecuteAsync<List<List<string>>>(Convert);

        private List<Trade> Convert(List<List<string>> data)
        {
            var trades = new List<Trade>();

            foreach (List<string> item in data)
            {
                Trade trade = _tradesConverter.FromListString(item);
                trades.Add(trade);
            }

            return trades;
        }
    }
}
