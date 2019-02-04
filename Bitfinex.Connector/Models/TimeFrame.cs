using System;
using System.Collections.Generic;

namespace Bitfinex.Connector.Models
{
    internal class TimeFrame
    {
        private static Dictionary<double, TimeFrame> _timeFrames = new Dictionary<double, TimeFrame>();

        public string Value { get; }

        public static TimeFrame Minute1 { get; } = new TimeFrame("1m", minutes: 1);
        public static TimeFrame Minute5 { get; } = new TimeFrame("5m", minutes: 5);
        public static TimeFrame Minute15 { get; } = new TimeFrame("15m", minutes: 15);
        public static TimeFrame Minute30 { get; } = new TimeFrame("30m", minutes: 30);
        public static TimeFrame Hour1 { get; } = new TimeFrame("1h", hours: 1);
        public static TimeFrame Hour3 { get; } = new TimeFrame("3h", hours: 3);
        public static TimeFrame Hour6 { get; } = new TimeFrame("6h", hours: 6);
        public static TimeFrame Hour12 { get; } = new TimeFrame("12h", hours: 12);
        public static TimeFrame Day1 { get; } = new TimeFrame("1D", days: 1);
        public static TimeFrame Day7 { get; } = new TimeFrame("7D", days: 7);
        public static TimeFrame Day14 { get; } = new TimeFrame("14D", days: 14);

        private TimeFrame(string value, int minutes = 0, int hours = 0, int days = 0)
        {
            Value = value;
            _timeFrames.Add(new TimeSpan(days, hours, minutes, 0, 0).TotalSeconds, this);
        }

        public override string ToString() => Value;

        public static TimeFrame Find(int seconds)
        {
            _timeFrames.TryGetValue(seconds, out TimeFrame timeFrame);
            return timeFrame;
        }
    }
}
