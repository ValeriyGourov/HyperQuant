using System;
using System.Threading.Tasks;
using Bitfinex.Connector.Models;
using Bitfinex.Connector.Models.RestRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitfinex.Connector.Tests.Models.Request
{
    [TestClass]
    public class CandlesHystoryRequestTests
    {
        [TestMethod]
        public async Task CandlesHystoryRequest_Test()
        {
            const int periodInSec = 60;
            const long count = 100;

            TimeFrame timeFrame = TimeFrame.Find(periodInSec);
            if (timeFrame == null)
            {
                throw new ArgumentException("Указан неправильный период.", nameof(periodInSec));
            }

            const string symbol = "tBTCUSD";
            CandlesHystoryRequest candlesHystoryRequest = new CandlesHystoryRequest(timeFrame, symbol)
            {
                Limit = count
            };

            var result = await candlesHystoryRequest.ExecuteAsync();
        }
    }
}
