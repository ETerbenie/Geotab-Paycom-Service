using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTC.Agent;
using Paycom;
using Paycom.Adapters;
using PTC;
using GTPC.Service.Implementation;
using Geotab.Checkmate.ObjectModel;
using GTPC.Service.Entities.Models;
using GTPC.Service.Implementation.Models;
using GTPC.Service.Models;
using GTPC.Service.Implementation.Implementation;
using Geotab;
using Paycom.Managers;

namespace GTPC.Service
{
    public class PaycomTasks : PTC.Agent.Task
    {
        CsvManipulation csvManip = new CsvManipulation();
        EmailUtility emailUtility = new EmailUtility();
        LogsManager logcycle = new LogsManager();
        CreateTimeCardManager timeCardsUtility = new CreateTimeCardManager();

        public PaycomTasks(Worker worker) : base(worker)
        {
            var pollingcycle = AppSettings.GetIntValue("PaycomTaskPollCycle", 28800);
            RunInterval = TimeSpan.FromSeconds(pollingcycle);

            OnRunLoop += PaycomTask_OnRunLoop;
            enabled = true;

        }

        private void PaycomTask_OnRunLoop()
        {
            Log.Info("Starting Process to create punches for Paycom...");
            PaycomAPIAdapter.SendPunchesToPaycom();

            Log.Info("Updating LastRunFile...");
            LastRecordedTime.UpdateLastRunFile(DateTime.UtcNow);

            Log.Info($"Running CsvTask at {DateTime.Now}");
            IEnumerable<DutyStatusLog> geotabLogs = logcycle.LogRecordsToCompare();
            IEnumerable<GeotabInfoModel> geotabInfo = logcycle.GetGeotabInfo(geotabLogs.ToList());
            IEnumerable<PaycomInfoModel> paycomInfo = timeCardsUtility.CreatePaycomInfoModel(geotabInfo);
            IEnumerable<TimeCardModel> mismatchPunches = timeCardsUtility.CompareGeoToPay(paycomInfo, geotabInfo);
            //Create a new method for 
            csvManip.ExportResultsToCsv(mismatchPunches);
            emailUtility.SendEmailWithCsvAttached();
        }

        public override TaskSchedule Schedule
        {
            get { return TaskSchedule.RunAtInterval; }
        }
        protected override void Load()
        {
            Log.Trace($"Task {GetType().Name} loaded");
        }

        protected override void UnLoad()
        {
            Log.Trace($"Task {GetType().Name} unloaded");
        }

        //protected virtual void StartTimer()
        //{
        //    timer = new PTC.Timer();
        //    timer.Interval = GetTimeInterval(); // TimeSpan.FromMinutes(CloudAgentAppSettings.MessagePurgeCustomerLockExpirationInMinutes);
        //    timer.OnInterval += timer_OnInterval;
        //    timer.Recurring = true;
        //    timer.Start();
        //}

        //protected TimeSpan GetTimeInterval()
        //{
        //    return TimeSpan.FromSeconds(AppSettings.GetIntValue("TaskIntervalInSeconds", 3600));
        //}

        //private void timer_OnInterval(object eventObject)
        //{
        //    Log.MethodStart();
        //    try
        //    {
        //        Run();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex);
        //    }
        //    finally
        //    {
        //        Log.ClearThreadName();
        //    }
        //}

        //protected override void Run()
        //{
        //    Log.Trace("Starting Process to create punches for Paycom");
        //    PaycomAPIAdapter.SendPunchesToPaycom();

        //    Log.Trace("Updating LastRunFile");
        //    LastRecordedTime.UpdateLastRunFile(DateTime.UtcNow);
        //}

        //protected override void UnLoad()
        //{
        //    Log.Trace($"Task {GetType().Name} unloaded");
        //}
    }
}
