using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitfinex.Connector.Models;
using Bitfinex.Connector.Models.RestRequest;
using Bitfinex.Connector.Models.WebSockeSubscription;
using Bitfinex.Domain;

namespace Bitfinex.Connector
{
    public class BitfinexConnector : ITestConnector
    {
        #region Rest

        public Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            TradesRequest tradesRequest = new TradesRequest(pair)
            {
                Limit = maxCount
            };

            return tradesRequest.ExecuteAsync();
        }

        public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            TimeFrame timeFrame = GetTimeFrame(periodInSec);
            CandlesHystoryRequest candlesHystoryRequest = new CandlesHystoryRequest(timeFrame, pair)
            {
                Limit = count.Value,
                Start = from == null ? default : from.Value.Millisecond,
                End = to == null ? default : to.Value.Millisecond
            };

            return candlesHystoryRequest.ExecuteAsync();
        }

        #endregion

        #region Socket

        private readonly Dictionary<string, TradesSubscription> _tradesSubscriptions = new Dictionary<string, TradesSubscription>();
        private readonly Dictionary<string, CandlesSubscription> _candlesSubscriptions = new Dictionary<string, CandlesSubscription>();

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            // TODO: Выяснить предназначение maxCount.
            TradesSubscription tradesSubscription = new TradesSubscription(pair)
            {
                NewBuyTradeEventRaiser = trade => NewBuyTrade?.Invoke(trade),
                NewSellTradeEventRaiser = trade => NewSellTrade?.Invoke(trade)
            };
            _tradesSubscriptions.Add(pair, tradesSubscription);

            tradesSubscription.ConnectAsync().GetAwaiter();
        }

        public void UnsubscribeTrades(string pair)
        {
            if (_tradesSubscriptions.TryGetValue(pair, out TradesSubscription tradesSubscription))
            {
                tradesSubscription.Dispose();
                _tradesSubscriptions.Remove(pair);
            }
        }

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            TimeFrame timeFrame = GetTimeFrame(periodInSec);
            CandlesSubscription candlesSubscription = new CandlesSubscription(timeFrame, pair)
            {
                CandleSeriesProcessingEventRaiser = candle => CandleSeriesProcessing?.Invoke(candle)
            };
            _candlesSubscriptions.Add(pair, candlesSubscription);

            candlesSubscription.ConnectAsync().GetAwaiter();
        }

        public void UnsubscribeCandles(string pair)
        {
            throw new NotImplementedException();
        }

        #endregion

        private TimeFrame GetTimeFrame(int periodInSec)
        {
            TimeFrame timeFrame = TimeFrame.Find(periodInSec);
            if (timeFrame == null)
            {
                throw new ArgumentException("Указан неправильный период.", nameof(periodInSec));
            }

            return timeFrame;
        }
    }
}
