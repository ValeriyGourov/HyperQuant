using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bitfinex.Connector.Models;
using Bitfinex.Connector.Models.RestRequest;
using Bitfinex.Domain;

namespace Bitfinex.Connector
{
    public class BitfinexConnector : ITestConnector
    {
        #region Rest

        public Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            const string symbol = "tBTCUSD";
            TradesRequest tradesRequest = new TradesRequest(symbol)
            {
                Limit = maxCount
            };

            return tradesRequest.ExecuteAsync();
        }

        public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            TimeFrame timeFrame = TimeFrame.Find(periodInSec);
            if (timeFrame == null)
            {
                throw new ArgumentException("Указан неправильный период.", nameof(periodInSec));
            }

            const string symbol = "tBTCUSD";
            CandlesHystoryRequest candlesHystoryRequest = new CandlesHystoryRequest(timeFrame, symbol)
            {
                Limit = count.Value,
                Start = from == null ? default : from.Value.Millisecond,
                End = to == null ? default : to.Value.Millisecond
            };

            return candlesHystoryRequest.ExecuteAsync();
        }

        #endregion

        #region Socket

        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            throw new NotImplementedException();
        }

        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeCandles(string pair)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeTrades(string pair)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
