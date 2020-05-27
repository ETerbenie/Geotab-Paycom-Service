using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTPC.Service.Implementation.Extensions
{
    public static class DateTimeExtensions
    {
        private static TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        private static TimeZoneInfo centralZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
        private static TimeZoneInfo mountainZone = TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
        private static TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

        public static DateTime ConvertToUtcFromEastern(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, easternZone);
        }

        public static DateTime ConvertToEasternFromUtc(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(dateTime, easternZone);
        }

        public static DateTime ConvertToUtcFromCentral(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, centralZone);
        }

        public static DateTime ConvertToUtcFromMountain(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, mountainZone);
        }

        public static DateTime ConvertToUtcFromPacific(this DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, pacificZone);
        }

        public static long DateTimetoUnixTimeStamp(this DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
            //return (System.TimeZoneInfo.ConvertTimeToUtc(dateTime) - new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;

        }

        public static DateTime Last14Days(DateTime date)
        {
            DateTime newDate = DateTime.UtcNow.AddDays(-14);
            return newDate;
        }
    }
}
