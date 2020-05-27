using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace GTPC.Service.Implementation
{
    public class LastRecordedTime
    {
        public static void UpdateLastRunFile(DateTime dateTime)
        {
            File.WriteAllText("C:\\LastRun.txt", dateTime.ToString());
        }

        public DateTime ReadLastRunFile()
        {
            var path = AppSettings.GetStringValue("");

            var fileText = File.ReadAllText("C:\\LastRun.txt");

            DateTime date;
            var success = DateTime.TryParse(fileText, out date);

            if (!success)
                return DateTime.UtcNow.AddDays(-1);

            return date;
        }
    }
}
