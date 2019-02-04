using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bitfinex.Connector.Infrastructure;
using Bitfinex.Domain;

[assembly: InternalsVisibleTo("Bitfinex.Connector.Tests")]

namespace Bitfinex.Connector.Models.Request
{
    internal class TradesRequest : RequestBase<IEnumerable<Trade>>
    {
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
        }

        public override Task<IEnumerable<Trade>> ExecuteAsync() => ExecuteAsync<List<List<string>>>(Convert);

        private List<Trade> Convert(List<List<string>> data)
        {
            const int minRowNumber = 4;
            const string tradeDirectionBuy = "buy";
            const string tradeDirectionSell = "sell";

            var trades = new List<Trade>();

            foreach (List<string> item in data)
            {
                if (item.Count < minRowNumber)
                {
                    throw new ApplicationException("Неверный формат ответа.");
                }

                long mts = long.Parse(item[1]);
                float amount = float.Parse(item[2]);

                Trade trade = new Trade()
                {
                    Id = item[0],
                    Pair = Symbol.Label,
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(mts),
                    Amount = System.Convert.ToDecimal(Math.Abs(amount)),
                    Side = amount > 0 ? tradeDirectionBuy : tradeDirectionSell
                };

                if (Symbol.Type == SymbolType.TradingPair)
                {
                    float price = float.Parse(item[3]);
                    trade.Price = System.Convert.ToDecimal(price);
                }

                trades.Add(trade);
            }

            return trades;
        }
    }
}
