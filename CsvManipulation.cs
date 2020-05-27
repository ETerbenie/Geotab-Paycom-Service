using GTPC.Service.Models;
using PTC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GTPC.Service.Implementation.Implementation
{
    public class CsvManipulation
    {

        public void ExportResultsToCsv(IEnumerable<TimeCardModel> nonMatchingunches)
        {
            Log.MethodStart();

            try
            {
                StringBuilder sb = new StringBuilder();

                if (File.Exists(@"C:\PunchReport.csv"))
                {
                    File.Delete(@"C:\PunchReport.csv");
                    File.Create(@"C:\PunchReport.csv").Dispose();
                }

                sb.Append($"First Name, Last Name, Employee Number, PunchType, DateTime\r\n");

                foreach (TimeCardModel incorrectPunch in nonMatchingunches)
                {
                    sb.Append($"{incorrectPunch.FirstName}, {incorrectPunch.LastName}, {incorrectPunch.EmployeeNumber}, {incorrectPunch.PunchType}, {incorrectPunch.PunchDate}\r\n");
                }

                byte[] byteArray = Encoding.ASCII.GetBytes(sb.ToString());

                using (FileStream stream = File.Open(@"C:\PunchReport.csv", FileMode.Open, FileAccess.Write))
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                }
            }
            catch (Exception)
            {

                throw;
            }
            //try
            //{
            //    if (File.Exists(@"C:\PunchReport.csv"))
            //        File.Delete(@"C:\PunchReport.csv");

            //    Log.Trace("Writing to PunchReport");
            //    File.AppendAllText(@"C:\PunchReport.csv", $"Employee Number, PunchType, DateTime\r\n");

            //    foreach (var incorrectPunch in nonMatchingunches)
            //    {
            //        File.AppendAllText(@"C:\PunchReport.csv", $"{incorrectPunch.EmployeeNumber}, {incorrectPunch.PunchType}, {incorrectPunch.PunchDate}\r\n");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex);
            //    throw;
            //}
        }


    }
}
