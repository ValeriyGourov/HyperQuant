using System.Text;

namespace Bitfinex.Connector.Models.WebSockeSubscription
{
    internal static class Helper
    {
        public static byte[] GetBytes(string data) => Encoding.UTF8.GetBytes(data);

        public static string GetString(byte[] data, int count) => Encoding.UTF8.GetString(data, 0, count);

        public static string GetString(byte[] data) => GetString(data, data.Length);
    }
}