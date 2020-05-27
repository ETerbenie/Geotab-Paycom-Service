using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TransfloExpress.Services;
using Geotab;
using GTPC.Service.Entities.Models;
using GTPC.Service.Implementation.Models;
using GTPC.Service.Implementation;
using PTC;
using GTPC.Service.Models;
using GTPC.Service.Implementation.Extensions;
using System.IO;
using System.Net.Mime;
using System.Net.Mail;
using Geotab.Checkmate.ObjectModel;
using Paycom.Managers;

namespace Paycom.Adapters
{
    public class PaycomAPIAdapter
    {
        // Methods
        public const string GET = "GET";
        public const string POST = "POST";

        // API Urls and Auth Token
        public static string baseUrl = AppSettings.GetStringValue("paycomBaseUrl");
        public static string AUTH_HEADER = AppSettings.GetStringValue("SIDToken");
        public static string punchImport = AppSettings.GetStringValue("punchImportURL");
        public static string punchHistory = AppSettings.GetStringValue("punchHistoryURL");
        public static string punchHistoryWithDate = AppSettings.GetStringValue("punchHistoryWithDateURL");
        public static string punchAuditWithDate = AppSettings.GetStringValue("punchAuditWithDateURL");

        // Global variable for employee number for urls
        public static object employeeNumber { get; set; }

        LastRecordedTime startDate;

        public PaycomAPIAdapter()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", AUTH_HEADER);
            startDate = new LastRecordedTime();
        }

        public static async void SendPunchesToPaycom()
        {
            Log.MethodStart();

            CreateTimeCardManager timeCardList = new CreateTimeCardManager();

            try
            {
                IEnumerable<TimeCardModel> list = timeCardList.CreatePunches();

                List<PaycomPostModel> uploadList = new List<PaycomPostModel>();

                foreach (TimeCardModel card in list)
                {
                    PaycomPostModel sendPaycom = new PaycomPostModel()
                    {
                        eecode = card.EmployeeNumber,
                        clocktype = card.ClockType,
                        punchtime = card.PunchTime.ToString(),
                        punchtype = card.PunchType,
                        entrytype = "1",
                        timezone = card.Timezone
                    };

                    uploadList.Add(sendPaycom);
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("Authorization", AUTH_HEADER);
                    string json = JsonConvert.SerializeObject(uploadList);
                    HttpContent content = new StringContent(json, Encoding.UTF8, mediaType: "application/json");

                    Task<HttpResponseMessage> result = client.PostAsync(punchImport, content);
                    
                    string resultData = await result.Result.Content.ReadAsStringAsync();
                   
                    Log.Info($"This is what will sent to Paycom...{json}");

                    if (result.Result.StatusCode == HttpStatusCode.Conflict)
                        Log.Info(result.ToString());

                    if (result.Result.StatusCode != HttpStatusCode.OK) //change to paycom's successful response code
                        Log.Info(resultData);
                }

                Log.MethodEnd();
            }

            catch(Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        public GeotabInfoModel IdentifyLastPunch(GeotabInfoModel driver)
        {
            Log.MethodStart();

            employeeNumber = driver.EmployeeNumber;
            Log.Info($"Getting last punch for {employeeNumber}");

            GeotabInfoModel lastEntry = new GeotabInfoModel();
            DateTime startDate = DateTime.Now.AddDays(-14).Date;

            string url = string.Format(punchAuditWithDate, employeeNumber, startDate.DateTimetoUnixTimeStamp());

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("Authorization", AUTH_HEADER);
                    HttpResponseMessage result = client.GetAsync(url).Result;

                    if (result.StatusCode  == HttpStatusCode.OK)
                    {
                        string resultData = result.Content.ReadAsStringAsync().Result;
                        PunchAuditResponseModel data = JsonConvert.DeserializeObject<PunchAuditResponseModel>(resultData);

                        if(data.errors.FirstOrDefault() == "No Content Found" || data.data.Active == null || data.data.Active.Length <= 0)
                        {
                            Log.Info($"{employeeNumber} has no punches in the last 14 days...");
                            string lastPunch = "OD";
                            lastEntry.PunchType = lastPunch;
                            return lastEntry;
                        }
                        else
                        {
                            Log.Info($"Last punches for employee {employeeNumber}...");
                            string lastPunch = data.data.Active.OrderByDescending(x => x.punchid).FirstOrDefault().punchtype;
                            lastEntry.PunchType = lastPunch;

                            return lastEntry;
                        }
                    }
                    else if (result.StatusCode == HttpStatusCode.PartialContent)
                    {
                        HttpHeaders headers = result.Headers;
                        IEnumerable<string> links;
                        headers.TryGetValues("Link", out links);

                        string listToString = links.First();
                        string[] uriArray = listToString.Split(',');
                        string newUri = uriArray.Take(uriArray.Length - 1).LastOrDefault();

                        int toRemove = newUri.IndexOf(">");
                        if (toRemove > 0)
                            newUri = newUri.Substring(0, toRemove);
                        newUri = newUri.Replace("<", "");
                        HttpResponseMessage newResult = client.GetAsync(newUri).Result;

                        string resultData = newResult.Content.ReadAsStringAsync().Result;
                        PunchHistoryResponseModel data = JsonConvert.DeserializeObject<PunchHistoryResponseModel>(resultData);
                        //Log.Info($"Last punches for employee {employeeNumber} : {resultData}");

                        string lastPunch = data.data.LastOrDefault().punchtype;
                        lastEntry.PunchType = lastPunch;

                        return lastEntry;

                    }

                    Log.MethodEnd();

                    return lastEntry;
                }
            }

            catch (Exception ex)
            {
                Log.Error($"{ex}");
                throw ex;
            }

        }

        public GeotabInfoModel AuditIdentifyLastPunch(GeotabInfoModel driver)
        {
            employeeNumber = driver.EmployeeNumber;

            GeotabInfoModel lastEntry = new GeotabInfoModel();
            var convertedStartDate = startDate.ReadLastRunFile().DateTimetoUnixTimeStamp();

            var url = string.Format(punchAuditWithDate, employeeNumber, convertedStartDate);

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("Authorization", AUTH_HEADER);
                    var result = client.GetAsync(url).Result;

                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        string resultData = result.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<PunchAuditResponseModel>(resultData);

                        string lastPunch = data.data.Active.OrderBy(x => x.punchid).LastOrDefault().punchtype;
                        lastEntry.PunchType = lastPunch;

                        return lastEntry;

                    }
                    else if (result.StatusCode == HttpStatusCode.PartialContent)
                    {
                        HttpHeaders headers = result.Headers;
                        IEnumerable<string> links;
                        headers.TryGetValues("Link", out links);

                        string listToString = links.First();
                        string[] uriArray = listToString.Split(',');
                        string newUri = uriArray.Take(uriArray.Length - 1).LastOrDefault();

                        int toRemove = newUri.IndexOf(">");
                        if (toRemove > 0)
                            newUri = newUri.Substring(0, toRemove);
                        newUri = newUri.Replace("<", "");
                        var newResult = client.GetAsync(newUri).Result;

                        string resultData = newResult.Content.ReadAsStringAsync().Result;
                        var data = JsonConvert.DeserializeObject<PunchHistoryResponseModel>(resultData);

                        var lastPunch = data.data.LastOrDefault().punchtype;
                        lastEntry.PunchType = lastPunch;

                        return lastEntry;

                    }

                    return lastEntry;
                }
            }

            catch (Exception ex)
            {
                Log.Error($"{ex}");
                throw ex;
            }

        }


        public IEnumerable<TimeCardModel> LastRecordedPunches(GeotabInfoModel driver)
        {
            Log.MethodStart();

            List<TimeCardModel> driverTimePunches = new List<TimeCardModel>();
            LastRecordedTime lastTime = new LastRecordedTime();

            employeeNumber = driver.EmployeeNumber;

            DateTime tempDate = lastTime.ReadLastRunFile().AddDays(-14);
            //Log.Info($"Getting paycom punches from {tempDate} for {employeeNumber}...");
            long startDate = DateTimeExtensions.DateTimetoUnixTimeStamp(tempDate);

            string url = string.Format(punchAuditWithDate, employeeNumber, startDate);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.DefaultRequestHeaders.Add("Authorization", AUTH_HEADER);
                    HttpResponseMessage result = client.GetAsync(url).Result;

                    if (result.StatusCode != HttpStatusCode.OK && result.StatusCode != HttpStatusCode.PartialContent)
                    {
                        return null;
                    }

                    string resultData = result.Content.ReadAsStringAsync().Result;

                    PunchAuditResponseModel data = JsonConvert.DeserializeObject<PunchAuditResponseModel>(resultData);
                    if(data.errors.FirstOrDefault() == "No Content Found")
                    {
                        Log.Info($"No recent punches for {employeeNumber} since {tempDate}...");
                        return null;
                    }


                    ActiveResponseModel[] punchData = data.data.Active;

                    foreach (var activePunch in punchData)
                    {
                        TimeCardModel currentTimeCard = new TimeCardModel();
                        //var change = TimeSpan.Parse(activePunch.punchtime.TrimEnd(new char[] { ' ', 'A', 'M', 'P' }));
                        DateTime temporaryDate = DateTime.Parse($"{activePunch.punchdate} {activePunch.punchtime}");

                        currentTimeCard.FirstName = driver.FirstName;
                        currentTimeCard.LastName = driver.LastName;
                        currentTimeCard.EmployeeNumber = driver.EmployeeNumber;
                        currentTimeCard.PunchDateTimeInUnix = TimeZoneConverter.ConvertToUtcFromTimeZone(driver.Timezone, temporaryDate);
                        //currentTimeCard.PunchDateTimeInUnix = DateTime.Parse($"{activePunch.punchdate} {activePunch.punchtime}").ConvertToUtcFromEastern().DateTimetoUnixTimeStamp();
                        currentTimeCard.PunchDate = $"{activePunch.punchdate} {activePunch.punchtime}";
                        currentTimeCard.PunchType = activePunch.punchtype;

                        driverTimePunches.Add(currentTimeCard);
                    }

                    //Log.Info($"Last punch for employee {employeeNumber} : {resultData}");

                    //driverTimePunches = punchData.Select(x => new TimeCardModel
                    //{
                    //    EmployeeNumber = driver.EmployeeNumber,
                    //    PunchTime = Convert.ToInt64(x.punchtime.TrimEnd(new char[] { ' ', 'A', 'M', 'P' })),
                    //    PunchDate = x.punchdate,
                    //    PunchType = x.punchtype
                    //}).ToList();
                }

                return driverTimePunches.OrderBy(x => x.PunchDate).OrderBy(y => y.PunchTime);
            }
            
            catch (Exception ex)
            {
                Log.Error(ex);
                Log.Info(ex);
                throw;
            }
        }
    }
}
