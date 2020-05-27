//using Geotab;
//using Geotab.Checkmate.ObjectModel;
//using GTPC.Service.Entities.Models;
//using GTPC.Service.Implementation.Implementation;
//using GTPC.Service.Implementation.Models;
//using GTPC.Service.Models;
//using Paycom.Adapters;
//using Paycom.Managers;
//using PTC;
//using PTC.Agent;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GTPC.Service
//{
//    public class CsvTask : PTC.Agent.Task
//    {
//        CsvManipulation csvManip = new CsvManipulation();
//        EmailUtility emailUtility = new EmailUtility();
//        LogsManager logcycle = new LogsManager();
//        CreateTimeCardManager timeCardsUtility = new CreateTimeCardManager();

//        public override TaskSchedule Schedule
//        {
//            get { return TaskSchedule.RunAtInterval; }
//        }

//        protected PTC.Timer timer;

//        public CsvTask(Worker worker) : base(worker)
//        {
//            Log.MethodStart();

//            var pollingCycle = AppSettings.GetIntValue("CsvTaskPollCycle", 3600);

//            RunInterval = TimeSpan.FromSeconds(pollingCycle);

//            OnRunLoop += CsvTask_OnRunLoop;
//            enabled = true;
//        }

//        private void CsvTask_OnRunLoop()
//        {
//            Log.MethodStart();
//            Log.Info($"Running CsvTask at {DateTime.Now}");
//            IEnumerable<DutyStatusLog> geotabLogs = logcycle.LogRecordsToCompare();
//            IEnumerable<GeotabInfoModel> geotabInfo = logcycle.GetGeotabInfo(geotabLogs.ToList());
//            IEnumerable<PaycomInfoModel> paycomInfo = timeCardsUtility.CreatePaycomInfoModel(geotabInfo);
//            IEnumerable<TimeCardModel> mismatchPunches = timeCardsUtility.CompareGeoToPay(paycomInfo, geotabInfo);
//            //Create a new method for 
//            csvManip.ExportResultsToCsv(mismatchPunches);
//            emailUtility.SendEmailWithCsvAttached();
//        }

//        protected override void Load()
//        {
//            Log.Trace($"Task {GetType().Name} loaded");
//        }

//        protected override void UnLoad()
//        {
//            Log.Info($"Task {GetType().Name} unloaded");
//        }

//        //protected virtual void StartTimer()
//        //{
//        //    timer = new PTC.Timer();
//        //    timer.Interval = GetTimeInterval(); // TimeSpan.FromMinutes(CloudAgentAppSettings.MessagePurgeCustomerLockExpirationInMinutes);
//        //    timer.OnInterval += timer_OnInterval;
//        //    timer.Recurring = true;
//        //    timer.Start();
//        //}

//        //protected TimeSpan GetTimeInterval()
//        //{
//        //    return TimeSpan.FromSeconds(AppSettings.GetIntValue("TaskIntervalInSeconds", 3600));
//        //}

//        //private void timer_OnInterval(object eventObject)
//        //{
//        //    Log.MethodStart();
//        //    try
//        //    {
//        //        Run();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Log.Error(ex);
//        //    }
//        //    finally
//        //    {
//        //        Log.ClearThreadName();
//        //    }
//        //}




//        //protected override void Run()
//        //{
//        //    var geotabLogs = logcycle.LogRecordsToCompare();
//        //    var geotabInfo = logcycle.GetGeotabInfo(geotabLogs.ToList());
//        //    var paycomInfo = timeCardsUtility.CreatePaycomInfoModel(geotabInfo);
//        //    var mismatchPunches = timeCardsUtility.CompareGeoToPay(paycomInfo, geotabInfo);
//        //    //Create a new method for 
//        //    csvManip.ExportResultsToCsv(mismatchPunches);
//        //    emailUtility.SendEmailWithCsvAttached();

//        //}


//    }
//}
