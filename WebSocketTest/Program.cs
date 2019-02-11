using System;
using Bitfinex.Connector;
using Bitfinex.Domain;

namespace WebSocketTest
{
    internal class Program
    {
        private static ITestConnector _testConnector;

        private static void Main(string[] args)
        {
            const string symbol = "tBTCUSD";
            const int periodInSec = 60;
            _testConnector = new BitfinexConnector();

            _testConnector.NewBuyTrade += TestConnector_NewBuyTrade;
            _testConnector.NewSellTrade += TestConnector_NewSellTrade;
            _testConnector.SubscribeTrades(symbol);

            _testConnector.CandleSeriesProcessing += TestConnector_CandleSeriesProcessing;
            _testConnector.SubscribeCandles(symbol, periodInSec);

            Console.ReadKey(false);
        }

        private static void TestConnector_CandleSeriesProcessing(Candle obj)
        {
            Console.WriteLine($"Candle {{ OpenTime: {obj.OpenTime}, Pair: {obj.Pair}, OpenPrice: {obj.OpenPrice}, ClosePrice: {obj.ClosePrice} }}");
        }

        private static void TestConnector_NewSellTrade(Trade obj)
        {
            Console.WriteLine($"SellTrade {{ Id: {obj.Id}, Pair: {obj.Pair}, Time: {obj.Time}, Side: {obj.Side}, Price: {obj.Price}, Amount: {obj.Amount} }}");
        }

        private static void TestConnector_NewBuyTrade(Trade obj)
        {
            Console.WriteLine($"BuyTrade {{ Id: {obj.Id}, Pair: {obj.Pair}, Time: {obj.Time}, Side: {obj.Side}, Price: {obj.Price}, Amount: {obj.Amount} }}");
        }
    }
}
