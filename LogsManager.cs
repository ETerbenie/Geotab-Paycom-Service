using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTC;
using GeoTab.ELD;
using Geotab.Checkmate;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;
using GeoTabProcessor;
using TransfloExpress.Services;
using GTPC.Service.Models;
using GTPC.Service.Entities.Models;
using GTPC.Service.Interfaces;
using GTPC.Service.Implementation;
using GTPC.Service.Implementation.Extensions;

namespace Geotab
{
    public class LogsManager
    {
        GeotabAPIAdapter geotabCalls;
        LastRecordedTime lastTime;
        

        public LogsManager()
        {
            geotabCalls = new GeotabAPIAdapter();
            lastTime = new LastRecordedTime();
        }

        public IEnumerable<GeotabInfoModel> GetGeotabInfo(List<DutyStatusLog> logList)
        {
            PTC.Log.MethodStart();

            List<GeotabInfoModel> listOfGeotabInfo = new List<GeotabInfoModel>();

            IEnumerable<Id> newList = logList.Select(l => l.Driver.Id).Distinct();
            PTC.Log.Info($"Found {newList.Count()} drivers found from logs...");

            foreach (Id driver in newList)
            {
                UserSearch driverSearch = new UserSearch
                {
                    IsDriver = true,
                    Id = driver,
                };

                User user = geotabCalls.GetUser(driverSearch);

                if(user.ActiveTo < DateTime.UtcNow)
                {
                    continue;
                }

                GeotabInfoModel geotabInfo = new GeotabInfoModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmployeeNumber = user.EmployeeNo,
                    Timezone = user.TimeZoneId.TimeZoneStringSpliter(), // starting the time zone
                    Logs = logList.Where(x => x.Driver.Id.Equals(user.Id)).ToList()
                };

                if(geotabInfo.EmployeeNumber != "" && geotabInfo.EmployeeNumber != null)
                {
                    listOfGeotabInfo.Add(geotabInfo);
                    PTC.Log.Info($"Adding employee {geotabInfo.EmployeeNumber} to upload list...");
                }

            }
            PTC.Log.Info($"Total of {listOfGeotabInfo.Count()} drivers that have employee numbers...");
            return listOfGeotabInfo;
        }

        public IEnumerable<GeotabInfoModel> GetGeotabInfo (List<User> driverList)
        {
            List<GeotabInfoModel> listOfGeotabInfo = new List<GeotabInfoModel>();

            List<DutyStatusLogType> bindLogTypes = LogTypesToUse();

            foreach (var driver in driverList)
            {
                PTC.Log.Info($"Associating {driver.Name} to the log list....");
                DutyStatusLogSearch statusSearch = new DutyStatusLogSearch
                {
                    UserSearch = new UserSearch { Id = driver.Id },
                    FromDate = lastTime.ReadLastRunFile(), // last run datetime, store in a temp file to obtain this, a separate have window service create it. once ask finshes, update file with last run date (dattimenow). Create new class (potentially interface) for it, separate methods that grabs stores and update
                    ToDate = DateTime.UtcNow,
                    Statuses = bindLogTypes,
                    IncludeBoundaryLogs = true
                };

                List<DutyStatusLog> logRecordList = geotabCalls.GetLogList(statusSearch).ToList();

                GeotabInfoModel geotabInfo = new GeotabInfoModel
                {
                    FirstName = driver.FirstName,
                    LastName = driver.LastName,
                    EmployeeNumber = driver.EmployeeNo,
                    Logs = logRecordList
                };

                if (geotabInfo.FirstName != "**<No User>")
                {
                    listOfGeotabInfo.Add(geotabInfo);
                }
            }
            return listOfGeotabInfo;
        }

        private List<DutyStatusLogType> LogTypesToUse()
        {
            List<DutyStatusLogType> bindLogTypes = new List<DutyStatusLogType>();
            bindLogTypes.Add(DutyStatusLogType.D);
            bindLogTypes.Add(DutyStatusLogType.ON);
            bindLogTypes.Add(DutyStatusLogType.YM);
            bindLogTypes.Add(DutyStatusLogType.OFF);
            bindLogTypes.Add(DutyStatusLogType.SB);
            bindLogTypes.Add(DutyStatusLogType.PC);
            bindLogTypes.Add(DutyStatusLogType.WT);

            return bindLogTypes;
        }

        

        public IEnumerable<DutyStatusLog> LogRecordsToCompare()
        {
            PTC.Log.MethodStart();
            PTC.Log.Info("Gathering log list....");
            List<DutyStatusLogType> bindLogTypes = LogTypesToUse();

            DutyStatusLogSearch statusSearch = new DutyStatusLogSearch
            {
                FromDate = lastTime.ReadLastRunFile().AddDays(-14), 
                ToDate = DateTime.UtcNow,
                Statuses = bindLogTypes,
                IncludeBoundaryLogs = true
            };

            List<DutyStatusLog> logRecordList = geotabCalls.GetLogList(statusSearch).ToList();

            PTC.Log.Info($"{logRecordList.Count()} logs found...");

            PTC.Log.MethodEnd();

            return logRecordList;
        }
    }

}

