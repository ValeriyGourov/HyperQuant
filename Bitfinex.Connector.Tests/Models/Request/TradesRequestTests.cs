using System.Threading.Tasks;
using Bitfinex.Connector.Models.Request;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitfinex.Connector.Tests.Models.Request
{
    [TestClass]
    public class TradesRequestTests
    {
        [TestMethod]
        public async Task TradesRequest_Test()
        {
            const string symbol = "tBTCUSD";
            TradesRequest tradesRequest = new TradesRequest(symbol)
            {
                Limit = 120,
                Sort = -1
            };

            var result = await tradesRequest.ExecuteAsync();
        }
    }
}
