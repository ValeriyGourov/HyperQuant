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
    internal class CandlesHystoryRequest : RequestBase<IEnumerable<Candle>>
    {
        private readonly CandleConverter _candleConverter;

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
            _candleConverter = new CandleConverter(Symbol);
        }

        public override Task<IEnumerable<Candle>> ExecuteAsync() => ExecuteAsync<List<List<string>>>(Convert);

        private List<Candle> Convert(List<List<string>> data)
        {
            var candles = new List<Candle>();

            foreach (List<string> item in data)
            {
                Candle candle = _candleConverter.FromListString(item);
                candles.Add(candle);
            }

            return candles;
        }
    }
}
