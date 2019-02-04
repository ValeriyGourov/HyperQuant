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
        private readonly TimeFrame _timeFrame;
        private readonly Symbol _symbol;

        [QueryStringParameter("limit")]
        public long Limit { get; set; }

        [QueryStringParameter("start")]
        public int Start { get; set; }

        [QueryStringParameter("end")]
        public int End { get; set; }

        [QueryStringParameter("sort")]
        public int Sort { get; set; }

        protected override sealed string EndpointName => "candles";

        protected override sealed string ResourceSuffix => "trade:{timeFrame}:{symbol}/hist";

        public CandlesHystoryRequest(TimeFrame timeFrame, string symbol)
        {
            _timeFrame = timeFrame;
            _symbol = new Symbol(symbol ?? throw new ArgumentNullException(nameof(symbol)));
        }

        public override async Task<IEnumerable<Candle>> ExecuteAsync()
        {
            _request.AddUrlSegment("timeFrame", _timeFrame);
            _request.AddUrlSegment("symbol", _symbol);
            List<List<string>> data = await GetDataAsync<List<List<string>>>().ConfigureAwait(false);
            if (data == null)
            {
                return null;
            }

            return Convert(data);
        }

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
                    Pair = _symbol.Label,
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
