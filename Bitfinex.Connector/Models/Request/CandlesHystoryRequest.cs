using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Bitfinex.Connector.Infrastructure;
using Bitfinex.Domain;

[assembly: InternalsVisibleTo("Bitfinex.Connector.Tests")]

namespace Bitfinex.Connector.Models.Request
{
    internal class CandlesHystoryRequest : RequestBase<IEnumerable<Candle>>
    {
        #region Параметры строки запроса

        [QueryParameter(QueryParameterType.QueryString)]
        public long Limit { get; set; }

        [QueryParameter(QueryParameterType.QueryString)]
        public int Start { get; set; }

        [QueryParameter(QueryParameterType.QueryString)]
        public int End { get; set; }

        [QueryParameter(QueryParameterType.QueryString)]
        public int Sort { get; set; }

        #endregion

        #region Параметры пути запроса

        [QueryParameter(QueryParameterType.UrlSegment)]
        private TimeFrame TimeFrame { get; }

        [QueryParameter(QueryParameterType.UrlSegment)]
        private Symbol Symbol { get; }

        #endregion

        protected override sealed string EndpointName => "candles";

        protected override sealed string ResourceSuffix => "trade:{timeFrame}:{symbol}/hist";

        public CandlesHystoryRequest(TimeFrame timeFrame, string symbol)
        {
            TimeFrame = timeFrame ?? throw new ArgumentNullException(nameof(timeFrame));
            Symbol = new Symbol(symbol ?? throw new ArgumentNullException(nameof(symbol)));
        }

        public override Task<IEnumerable<Candle>> ExecuteAsync() => ExecuteAsync<List<List<string>>>(Convert);

        private List<Candle> Convert(List<List<string>> data)
        {
            const int minRowNumber = 6;

            var candles = new List<Candle>();

            foreach (List<string> item in data)
            {
                if (item.Count < minRowNumber)
                {
                    throw new ApplicationException("Неверный формат ответа.");
                }

                long mts = long.Parse(item[0]);
                float open = float.Parse(item[1]);
                float close = float.Parse(item[2]);
                float high = float.Parse(item[3]);
                float low = float.Parse(item[4]);
                float volume = float.Parse(item[5]);

                Candle trade = new Candle()
                {
                    Pair = Symbol.Label,
                    OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(mts),
                    OpenPrice = System.Convert.ToDecimal(open),
                    ClosePrice = System.Convert.ToDecimal(close),
                    HighPrice = System.Convert.ToDecimal(high),
                    LowPrice = System.Convert.ToDecimal(low),
                    // TotalPrice - непонятно что это такое.
                    TotalVolume = System.Convert.ToDecimal(volume)
                };

                candles.Add(trade);
            }

            return candles;
        }
    }
}
