using System.Linq;
using System.Threading.Tasks;
using Bitfinex.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitfinex.Connector.Tests
{
    [TestClass]
    public class BitfinexConnectorTests
    {
        #region Rest

        [TestMethod]
        public async Task GetNewTradesAsync()
        {
            const string pair = "tBTCUSD";
            const int maxCount = 100;
            BitfinexConnector connector = new BitfinexConnector();

            var result = (await connector.GetNewTradesAsync(pair, maxCount)).ToList();

            Assert.IsTrue(result.Count <= maxCount);
            CollectionAssert.AllItemsAreNotNull(result);
            CollectionAssert.AllItemsAreInstancesOfType(result, typeof(Trade));
        }

        [TestMethod]
        public async Task GetCandleSeriesAsync()
        {
            const string pair = "tBTCUSD";
            const int periodInSec = 60;
            const long count = 100;
            BitfinexConnector connector = new BitfinexConnector();

            var result = (await connector.GetCandleSeriesAsync(pair, periodInSec, null, null, count)).ToList();

            Assert.IsTrue(result.Count <= count);
            CollectionAssert.AllItemsAreNotNull(result);
            CollectionAssert.AllItemsAreInstancesOfType(result, typeof(Candle));
        }

        #endregion

        #region Socket


        #endregion
    }
}
