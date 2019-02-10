using System;
using System.Collections.Generic;
using System.Globalization;
using Bitfinex.Connector.Models;
using Bitfinex.Domain;

namespace Bitfinex.Connector.Converters
{
    internal class TradeConverter
    {
        private readonly Symbol _symbol;

        public TradeConverter(Symbol symbol)
        {
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public Trade FromListString(List<string> data)
        {
            const int minRowNumber = 4;
            const string tradeDirectionBuy = "buy";
            const string tradeDirectionSell = "sell";

            if (data.Count < minRowNumber)
            {
                throw new FormatException("Неверный формат исходных данных.");
            }

            long mts = long.Parse(data[1]);
            float amount = float.Parse(data[2], CultureInfo.InvariantCulture);

            Trade trade = new Trade()
            {
                Id = data[0],
                Pair = _symbol.Label,
                Time = DateTimeOffset.FromUnixTimeMilliseconds(mts),
                Amount = Convert.ToDecimal(Math.Abs(amount)),
                Side = amount > 0 ? tradeDirectionBuy : tradeDirectionSell
            };

            if (_symbol.Type == SymbolType.TradingPair)
            {
                float price = float.Parse(data[3], CultureInfo.InvariantCulture);
                trade.Price = Convert.ToDecimal(price);
            }

            return trade;
        }
    }
}
