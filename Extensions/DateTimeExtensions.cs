using System;

namespace HawkvNext.Extensions
{
    internal static class DateTimeExtensions
    {
        internal static ulong UnixTimeMillis(this DateTime now)
        {
            DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan ts = now - epochStart;

            return Convert.ToUInt64(ts.TotalSeconds * 1000);
        }
    }
}