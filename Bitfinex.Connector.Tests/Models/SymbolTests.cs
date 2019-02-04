using System;
using Bitfinex.Connector.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bitfinex.Connector.Tests.Models
{
    [TestClass]
    public class SymbolTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Symbol_EmptySymbolString_ArgumentNullException()
        {
            string symbolString = string.Empty;
            Symbol symbol = new Symbol(symbolString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Symbol_NullSymbolString_ArgumentNullException()
        {
            string symbolString = null;
            Symbol symbol = new Symbol(symbolString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Symbol_WrongLengthTradingPair_ArgumentException()
        {
            const string symbolString = "tBTC";
            Symbol symbol = new Symbol(symbolString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Symbol_WrongLengthFundingCurrency_ArgumentException()
        {
            const string symbolString = "fBTCUSD";
            Symbol symbol = new Symbol(symbolString);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Symbol_WrongPrefix_ArgumentException()
        {
            const string symbolString = "aBTCUSD";
            Symbol symbol = new Symbol(symbolString);
        }

        [TestMethod]
        public void Symbol_TradingPairPrefix_TypeIsTradingPair()
        {
            const string symbolString = "tBTCUSD";
            Symbol symbol = new Symbol(symbolString);

            Assert.AreEqual(symbol.Type, SymbolTypes.TradingPair);
        }

        [TestMethod]
        public void Symbol_FundingCurrencyPrefix_TypeIsFundingCurrency()
        {
            const string symbolString = "fUSD";
            Symbol symbol = new Symbol(symbolString);

            Assert.AreEqual(symbol.Type, SymbolTypes.FundingCurrency);
        }

        [TestMethod]
        public void Symbol_DifferentCase_LabelIsTradingPair()
        {
            const string symbolString = "Tbtcusd";
            const string label = "BTCUSD";
            Symbol symbol = new Symbol(symbolString);

            Assert.AreEqual(symbol.Label, label);
        }

        [TestMethod]
        public void Symbol_DifferentCase_ToStringIs_tBTCUSD()
        {
            const string symbolString = "Tbtcusd";
            const string toString = "tBTCUSD";
            Symbol symbol = new Symbol(symbolString);

            Assert.AreEqual(symbol.ToString(), toString);
        }
    }
}
