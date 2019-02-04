using System.Threading.Tasks;
using Bitfinex.Connector.Models.RestRequest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitfinex.Connector.Tests.Models.RestRequest
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
