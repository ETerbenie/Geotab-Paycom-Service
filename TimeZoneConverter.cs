using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTPC.Service.Implementation.Extensions
{
    public static class TimeZoneConverter
    {
        public static string TimeZoneStringSpliter(this string timeZone)
        {
            if(timeZone == "America/New_York")
            {
                timeZone = "EST";
                return timeZone;
                
            }
            else if(timeZone != null)
            {
                string splitTimeZone = timeZone.Substring(0, 3);

                return splitTimeZone;
            }
            else
            {
                return "";
            }
            
        }

        public static long ConvertToUtcFromTimeZone(this string timeZone, DateTime date)
        {
            long punchTimeInUnix = 0;
            if (timeZone == "EST")
            {
                punchTimeInUnix = date.ConvertToUtcFromEastern().DateTimetoUnixTimeStamp();
                return punchTimeInUnix;
            }
            else if (timeZone == "CST")
            {
                punchTimeInUnix = date.ConvertToUtcFromCentral().DateTimetoUnixTimeStamp();
                return punchTimeInUnix;
            }
            else if (timeZone == "MST")
            {
                punchTimeInUnix = date.ConvertToUtcFromMountain().DateTimetoUnixTimeStamp();
                return punchTimeInUnix;
            }
            else if (timeZone == "PST")
            {
                punchTimeInUnix = date.ConvertToUtcFromPacific().DateTimetoUnixTimeStamp();
                return punchTimeInUnix;
            }
            else
            {
                return punchTimeInUnix;
            }
        }


        //if (driver.Timezone == "EST") // if it breaks place this back into PaycomAPIAdapter in last recordedpunches under the temporaryDate variable
        //{
        //    currentTimeCard.PunchDateTimeInUnix = temporaryDate.ConvertToUtcFromEastern().DateTimetoUnixTimeStamp();
        //}
        //else if(driver.Timezone == "CST")
        //{
        //    currentTimeCard.PunchDateTimeInUnix = temporaryDate.ConvertToUtcFromCentral().DateTimetoUnixTimeStamp();
        //}
        //else if(driver.Timezone == "MST")
        //{
        //    currentTimeCard.PunchDateTimeInUnix = temporaryDate.ConvertToUtcFromMountain().DateTimetoUnixTimeStamp();
        //}
        //else if(driver.Timezone == "PST")
        //{
        //    currentTimeCard.PunchDateTimeInUnix = temporaryDate.ConvertToUtcFromPacific().DateTimetoUnixTimeStamp();
        //}
    }
}
