using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bitfinex.Connector.Models.WebSockeSubscription
{
    [JsonConverter(typeof(StringEnumConverter), true)]
    internal enum RequestEvent
    {
        Subscribe,
        Unsubscribe
    }
}
