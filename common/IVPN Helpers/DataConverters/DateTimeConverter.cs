using System;

namespace IVPN_Helpers.DataConverters
{
    public class DateTimeConverter
    {
        private static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromUnixTime(Int64 unixTime)
        {
            return UnixEpochStart.AddSeconds(unixTime).ToLocalTime();
        }

        public static Int64 ToUnixTime(DateTime t)
        {
            return (Int64) (t.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
