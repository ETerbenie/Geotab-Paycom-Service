using Geotab.Checkmate.ObjectModel;
using GTPC.Service.Entities.Models;
using GTPC.Service.Implementation.Models;
using GTPC.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTPC.Service.Implementation.Extensions
{
    public static class GeotabExtensions
    {
        public static Dictionary<DutyStatusLogType, string> ConvertLogTypeToPunchType(this IEnumerable<DutyStatusLog> logs)
        {
            Dictionary<DutyStatusLogType, string> keyValuePairs = new Dictionary<DutyStatusLogType, string>();

            foreach (var log in logs)
            {
                if (keyValuePairs.ContainsKey(log.Status.Value))
                    continue;

                switch (log.Status)
                {
                    case DutyStatusLogType.ON:
                    case DutyStatusLogType.D:
                    case DutyStatusLogType.YM:
                        keyValuePairs.Add(log.Status.Value, "ID");
                        break;
                    case DutyStatusLogType.OFF:
                    case DutyStatusLogType.SB:
                    case DutyStatusLogType.PC:
                    case DutyStatusLogType.WT:
                        keyValuePairs.Add(log.Status.Value, "OD");
                        break;
                }
            }

            return keyValuePairs;
        }

        public static List<TimeCardModel> MapInfoToPunchType(this IEnumerable<GeotabInfoModel> models)
        {
            List<TimeCardModel> timeCards = new List<TimeCardModel>();

            foreach (var model in models)
            {
                foreach (var log in model.Logs)
                {
                    string punchType = string.Empty;

                    switch (log.Status)
                    {
                        case DutyStatusLogType.ON:
                        case DutyStatusLogType.D:
                        case DutyStatusLogType.YM:
                            punchType = "ID";
                            break;
                        case DutyStatusLogType.OFF:
                        case DutyStatusLogType.SB:
                        case DutyStatusLogType.PC:
                        case DutyStatusLogType.WT:
                            punchType = "OD";
                            break;
                    }

                    DateTime dateWithoutSeconds = log.DateTime.Value.AddSeconds(-log.DateTime.Value.Second);

                    TimeCardModel timeCard = new TimeCardModel
                    {
                        EmployeeNumber = model.EmployeeNumber,
                        PunchDateTimeInUnix = dateWithoutSeconds.DateTimetoUnixTimeStamp(),
                        PunchType = punchType
                    };

                    timeCards.Add(timeCard);
                }
            }

            return timeCards;
        }
    }
}
