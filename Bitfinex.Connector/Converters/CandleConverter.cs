using System;
using System.Collections.Generic;
using System.Globalization;
using Bitfinex.Connector.Models;
using Bitfinex.Domain;

namespace Bitfinex.Connector.Converters
{
    internal class CandleConverter
    {
        private readonly Symbol _symbol;

        public CandleConverter(Symbol symbol)
        {
            _symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        }

        public Candle FromListString(List<string> data)
        {
            const int minRowNumber = 6;

            if (data.Count < minRowNumber)
            {
                throw new FormatException("Неверный формат исходных данных.");
            }

            long mts = long.Parse(data[0]);
            float open = float.Parse(data[1], CultureInfo.InvariantCulture);
            float close = float.Parse(data[2], CultureInfo.InvariantCulture);
            float high = float.Parse(data[3], CultureInfo.InvariantCulture);
            float low = float.Parse(data[4], CultureInfo.InvariantCulture);
            float volume = float.Parse(data[5], CultureInfo.InvariantCulture);

            return new Candle()
            {
                Pair = _symbol.Label,
                OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(mts),
                OpenPrice = Convert.ToDecimal(open),
                ClosePrice = Convert.ToDecimal(close),
                HighPrice = Convert.ToDecimal(high),
                LowPrice = Convert.ToDecimal(low),
                // TotalPrice - непонятно что это такое.
                TotalVolume = Convert.ToDecimal(volume)
            };
        }
    }
}
