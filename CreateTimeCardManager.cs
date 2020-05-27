using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geotab;
using Geotab.Checkmate.ObjectModel;
using GeoTab.ELD;
using GTPC.Service.Models;
using GTPC.Service.Entities.Models;
using GTPC.Service.Implementation;
using GTPC.Service.Implementation.Extensions;
using GTPC.Service.Implementation.Models;
using PTC;
using Paycom.Adapters;

namespace Paycom.Managers
{
    public class CreateTimeCardManager
    {

        LogsManager logcycle;
        GeotabAPIAdapter geotabCalls;
        LastRecordedTime lastRunTime;
        PaycomAPIAdapter paycomCalls;

        public CreateTimeCardManager()
        {
            logcycle = new LogsManager();
            geotabCalls = new GeotabAPIAdapter();
            lastRunTime = new LastRecordedTime();
            paycomCalls = new PaycomAPIAdapter();
        }

        // change to the one method above
        public IEnumerable<TimeCardModel> CreatePunches()
        {
            Log.MethodStart();
            Log.Info("Creating log punches....");

            List<DutyStatusLogType> bindLogTypes = new List<DutyStatusLogType>();
            bindLogTypes.Add(DutyStatusLogType.D);
            bindLogTypes.Add(DutyStatusLogType.ON);
            bindLogTypes.Add(DutyStatusLogType.YM);
            bindLogTypes.Add(DutyStatusLogType.OFF);
            bindLogTypes.Add(DutyStatusLogType.SB);
            bindLogTypes.Add(DutyStatusLogType.PC);
            bindLogTypes.Add(DutyStatusLogType.WT);

            DutyStatusLogSearch logSearch = new DutyStatusLogSearch
            {
                FromDate = lastRunTime.ReadLastRunFile(), // last run datetime, store in a temp file to obtain this, a separate have window service create it. once ask finshes, update file with last run date (dattimenow). Create new class (potentially interface) for it, separate methods that grabs stores and update
                ToDate = DateTime.UtcNow,
                Statuses = bindLogTypes,
                IncludeBoundaryLogs = true
            };

            Log.Info($"Logsearch for {logSearch.FromDate} to {logSearch.ToDate}...");

            List<DutyStatusLog> listOfLogs = geotabCalls.GetLogList(logSearch).OrderBy(y => y.DateTime).OrderBy(x => x.Driver.Id).ToList();

            Log.Info($"{listOfLogs.Count()} logs were found...");

            List<GeotabInfoModel> listOfGeotabInfo = logcycle.GetGeotabInfo(listOfLogs).ToList();

            //TODO: not duplicating the duty status log types 
            List<TimeCardModel> punches = CreateTimeCard(listOfGeotabInfo);

            Log.MethodEnd();

            return punches;
        }



        private List<TimeCardModel> CreateTimeCard(List<GeotabInfoModel> listOfGeotabInfo)
        {
            Log.MethodStart();
            List<TimeCardModel> timeCards = new List<TimeCardModel>();

            foreach (GeotabInfoModel record in listOfGeotabInfo)
            {
                bool previousLogIsOnDuty = false;
                bool previousLogIsOffDuty = false;

                GeotabInfoModel oldPunch = paycomCalls.IdentifyLastPunch(record);

                //var auditLastPunch = paycomCalls.AuditIdentifyLastPunch(record); // testing with punchaudit url

                if (oldPunch.PunchType == "ID")
                {
                    previousLogIsOnDuty = true;
                    previousLogIsOffDuty = false;
                }
                else if (oldPunch.PunchType == "OD")
                {
                    previousLogIsOnDuty = false;
                    previousLogIsOffDuty = true;
                }

                record.Logs = record.Logs.OrderBy(l => l.DateTime).ToList();

                List<DutyStatusLog> logs = record.Logs;

                for (int i = 0; i < record.Logs.Count; i++)
                {
                    TimeCardModel timeCard = new TimeCardModel();

                    timeCard.FirstName = record.FirstName;
                    timeCard.LastName = record.LastName;
                    timeCard.EmployeeNumber = record.EmployeeNumber;
                    timeCard.Timezone = record.Timezone;
                    timeCard.ClockType = "S";
                    timeCard.EntryType = 1;

                    switch (logs[i].Status)
                    {
                        case DutyStatusLogType.D:
                        case DutyStatusLogType.ON:
                        case DutyStatusLogType.YM:
                            if (previousLogIsOnDuty)
                            {
                                continue;
                            }
                            else
                            {
                                timeCard.PunchType = "ID";
                                timeCard.PunchTime = logs[i].DateTime.Value.DateTimetoUnixTimeStamp();
                                timeCards.Add(timeCard);
                            }

                            previousLogIsOnDuty = true;
                            previousLogIsOffDuty = false;
                            break;
                        case DutyStatusLogType.OFF:
                        case DutyStatusLogType.PC:
                        case DutyStatusLogType.SB:
                        case DutyStatusLogType.WT:
                            if (previousLogIsOffDuty)
                            {
                                continue;
                            }
                            else
                            {
                                timeCard.PunchType = "OD";
                                timeCard.PunchTime = logs[i].DateTime.Value.DateTimetoUnixTimeStamp();
                                timeCards.Add(timeCard);
                            }

                            previousLogIsOffDuty = true;
                            previousLogIsOnDuty = false;
                            break;
                    }
                }
            };

            Log.MethodEnd();

            return timeCards;
        }

        public IEnumerable<PaycomInfoModel> CreatePaycomInfoModel(IEnumerable<GeotabInfoModel> geotabInfo)
        {
            Log.MethodStart();

            List<PaycomInfoModel> finalList = geotabInfo.Select(x => new PaycomInfoModel
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                EmployeeNumber = x.EmployeeNumber,
                PunchRecords = paycomCalls.LastRecordedPunches(x)?.ToList() ?? new List<TimeCardModel>()
            }).ToList();

            Log.Info($"Turning {finalList.Count()} into paycom list to compare...");

            return finalList;
        }

        public IEnumerable<TimeCardModel> CompareGeoToPay(IEnumerable<PaycomInfoModel> paycomInfo, IEnumerable<GeotabInfoModel> geotabInfo)
        {
            Log.MethodStart();
            //List<PaycomInfoModel> finalPaycomPunchList = new List<PaycomInfoModel>();
            List<TimeCardModel> paycomInfoMappedToTimeCard = paycomInfo.SelectMany(x => x.PunchRecords).ToList();
            List<TimeCardModel> geotabInfoMappedToTimeCard = geotabInfo.MapInfoToPunchType();

            //foreach (PaycomInfoModel paycomCard in paycomInfo)
            //{
            //    if (paycomCard.PunchType != null)
            //        finalPaycomPunchList.Add(paycomCard);
            //}

            // take the paycom list, converting that list to a new list of timecardmodels, where geotab log datetimes (converted to unix) do not match the paycom punch times in unix

            //IEnumerable<TimeCardModel> mismatchList = paycomInfo.SelectMany(x => x.PunchRecords.Where(y => !geotabInfo.SelectMany(a => a.Logs.Select(b => b.DateTime)).Select(b => b.Value.DateTimetoUnixTimeStamp()).Any(c => c.Equals(y.PunchDateTimeInUnix))));
            //IEnumerable<TimeCardModel> mismatchList = paycomInfo.SelectMany(x => x.PunchRecords.Where(y => !geotabInfo.SelectMany(a => a.Logs.Select(b => b.DateTime)).Select(b => b.Value.DateTimetoUnixTimeStamp()).Any(c => c.Equals(y.PunchDateTimeInUnix))));

            IEnumerable<TimeCardModel> mismatchList = paycomInfoMappedToTimeCard.FindAll(x => !geotabInfoMappedToTimeCard.Any(y => y.Equals(x)));

            Log.MethodEnd();
            Log.Info($"Number of users that had mismatch: {mismatchList.Count()}");
            return mismatchList;
        }
    }
}


