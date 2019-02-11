using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitfinex.Connector.Converters;
using Bitfinex.Domain;
using Newtonsoft.Json;

namespace Bitfinex.Connector.Models.WebSockeSubscription
{
    internal class CandlesSubscription : SubscriptionBase
    {
        private readonly CandleConverter _candleConverter;
        private Dictionary<DateTimeOffset, Candle> _snapshot;

        protected override string Channel => "candles";

        public Action<Candle> CandleSeriesProcessingEventRaiser { private get; set; }

        public CandlesSubscription(TimeFrame timeFrame, string symbol)
        {
            if (timeFrame == null)
            {
                throw new ArgumentNullException(nameof(timeFrame));
            }
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            Symbol symbolObj = new Symbol(symbol);
            _additionalInitialMessageItems = new Dictionary<string, object>
            {
                { "key", $"trade:{timeFrame}:{symbolObj}" }
            };

            _candleConverter = new CandleConverter(symbolObj);
        }

        protected override async Task ProcessThreeLevelData(JsonTextReader jsonTextReader)
        {
            List<Candle> candles = await ReadThreeLevelData(jsonTextReader, data => ConvertHandler(data));
            _snapshot = candles.ToDictionary(item => item.OpenTime);
        }

        protected override async Task ProcessTwoLevelData(JsonTextReader jsonTextReader)
        {
            Candle candle = await ReadTwoLevelData(jsonTextReader, data => ConvertHandler(data));
            _snapshot[candle.OpenTime] = candle;
            CandleSeriesProcessingEventRaiser?.Invoke(candle);
        }

        private Candle ConvertHandler(List<string> data) => _candleConverter.FromListString(data);
    }
}
