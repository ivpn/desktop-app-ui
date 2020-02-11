using System;

namespace IVPN_Helpers.DataConverters
{
    public class DateTimeConverter
    {
        private static readonly DateTime UnixEpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static DateTime FromUnixTime(long unixTime)
        {
            return UnixEpochStart.AddSeconds(unixTime);
        }
    }
}
