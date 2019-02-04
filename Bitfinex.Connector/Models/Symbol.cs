using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Bitfinex.Connector.Tests")]

namespace Bitfinex.Connector.Models
{
    internal class Symbol
    {
        private string _symbolString;

        public SymbolType Type { get; private set; }
        public string Label { get; private set; }

        public Symbol(string symbolString)
        {
            ProcessSymbolString(symbolString);
        }

        private void ProcessSymbolString(string symbolString)
        {
            if (string.IsNullOrWhiteSpace(symbolString))
            {
                throw new ArgumentNullException(nameof(symbolString));
            }

            const string tradingPairPrefix = "t";
            const string fundingCurrencyPrefix = "f";

            string prefix = symbolString.Substring(0, 1).ToLowerInvariant();
            switch (prefix)
            {
                case tradingPairPrefix:
                    Type = SymbolType.TradingPair;
                    break;
                case fundingCurrencyPrefix:
                    Type = SymbolType.FundingCurrency;
                    break;
                default:
                    throw new ArgumentException($"Префикс строки символа должен быть равен '{tradingPairPrefix}' или '{fundingCurrencyPrefix}'.", nameof(symbolString));
            }

            const int tradingPairLength = 7;
            const int fundingCurrencyLength = 4;
            string message = "Для символа типа '{0}' длина строки символа должна составлять {1} символов.";
            int length = 0;
            switch (Type)
            {
                case SymbolType.TradingPair:
                    length = tradingPairLength;
                    break;
                case SymbolType.FundingCurrency:
                    length = fundingCurrencyLength;
                    break;
            }
            if (symbolString.Length != length)
            {
                throw new ArgumentException(string.Format(message, Type, length), nameof(symbolString));
            }

            Label = symbolString.Substring(1).ToUpperInvariant();
            _symbolString = $"{prefix}{Label}";
        }

        public override string ToString() => _symbolString;
    }
}
