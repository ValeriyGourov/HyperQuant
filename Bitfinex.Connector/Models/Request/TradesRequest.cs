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
        private readonly Symbol _symbol;

        [QueryStringParameter("limit")]
        public int Limit { get; set; }

        [QueryStringParameter("start")]
        public int Start { get; set; }

        [QueryStringParameter("end")]
        public int End { get; set; }

        [QueryStringParameter("sort")]
        public int Sort { get; set; }

        protected override sealed string EndpointName => "trades";

        protected override sealed string ResourceSuffix => "{symbol}/hist";

        public TradesRequest(string symbol)
        {
            _symbol = new Symbol(symbol ?? throw new ArgumentNullException(nameof(symbol)));
        }

        public override async Task<IEnumerable<Trade>> ExecuteAsync()
        {
            _request.AddUrlSegment("symbol", _symbol);
            List<List<string>> data = await GetDataAsync<List<List<string>>>().ConfigureAwait(false);
            if (data == null)
            {
                return null;
            }

            return Convert(data);
        }

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
                    Pair = _symbol.Label,
                    Time = DateTimeOffset.FromUnixTimeMilliseconds(mts),
                    Amount = System.Convert.ToDecimal(Math.Abs(amount)),
                    Side = amount > 0 ? tradeDirectionBuy : tradeDirectionSell
                };

                if (_symbol.Type == SymbolTypes.TradingPair)
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
