using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitfinex.Connector.Converters;
using Bitfinex.Domain;
using Newtonsoft.Json;

namespace Bitfinex.Connector.Models.WebSockeSubscription
{
    internal class TradesSubscription : SubscriptionBase
    {
        private readonly TradeConverter _tradesConverter;
        private Dictionary<string, Trade> _snapshot;

        protected override string Channel => "trades";

        public Action<Trade> NewBuyTradeEventRaiser { private get; set; }
        public Action<Trade> NewSellTradeEventRaiser { private get; set; }

        public TradesSubscription(string symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            Symbol symbolObj = new Symbol(symbol);
            _additionalInitialMessageItems = new Dictionary<string, object>
            {
                { "symbol", symbolObj.ToString() }
            };

            _tradesConverter = new TradeConverter(symbolObj);
        }

        protected override async Task ProcessThreeLevelData(JsonTextReader jsonTextReader)
        {
            List<Trade> trades = await ReadThreeLevelData(jsonTextReader, data => ConvertHandler(data));
            _snapshot = trades.ToDictionary(item => item.Id);
        }

        protected override async Task ProcessTwoLevelData(JsonTextReader jsonTextReader)
        {
            Trade trade = await ReadTwoLevelData(jsonTextReader, data => ConvertHandler(data));
            _snapshot[trade.Id] = trade;

            const string tradeDirectionBuy = "buy";
            const string tradeDirectionSell = "sell";

            if (trade.Side == tradeDirectionBuy)
            {
                NewBuyTradeEventRaiser?.Invoke(trade);
            }
            else if (trade.Side == tradeDirectionSell)
            {
                NewSellTradeEventRaiser?.Invoke(trade);
            }
        }

        private Trade ConvertHandler(List<string> data) => _tradesConverter.FromListString(data);
    }
}
