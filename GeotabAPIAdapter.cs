using System;
using System.Collections.Generic;
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


namespace Geotab
{
    public class GeotabAPIAdapter
    {
        API api;
        public const string GET = "Get";

        public GeotabAPIAdapter()
        {
            string username = AppSettings.GetStringValue("username");
            string password = AppSettings.GetStringValue("password");
            string database = AppSettings.GetStringValue("database");
            string sessionId = AppSettings.GetStringValue("sessionId");
            string path = AppSettings.GetStringValue("path");

            api = new API(username, password, sessionId, database, path);
            api.AuthenticateAsync();

        }

        // Overload User search
        public IEnumerable<User> GetUserList()
        {
            var driverList = api.CallAsync<List<User>>(GET, typeof(User)).Result;
            return driverList;
        }

        public IEnumerable<User> GetUserList(UserSearch userSearch)
        {
            var driverList = api.CallAsync<List<User>>(GET, typeof(User), new { search = userSearch }).Result;
            return driverList;
        }

        // Single user search
        public User GetUser (UserSearch userSearch)
        {
            var user = api.CallAsync<List<User>>(GET, typeof(User), new { search = userSearch }).Result;
            return user.FirstOrDefault();
        }


        // Overload Device search
        public IEnumerable<Device> GetDeviceList()
        {
            var deviceList = api.CallAsync<List<Device>>(GET, typeof(Device)).Result;
            return deviceList;
        }

        public IEnumerable<Device> GetDeviceList(DeviceSearch deviceSearch)
        {
            var deviceList = api.CallAsync<List<Device>>(GET, typeof(Device), new { search = deviceSearch }).Result;
            return deviceList;
        }


        // Overload DutyStatusLog search
        public IEnumerable<DutyStatusLog> GetLogList()
        {
            var logList = api.CallAsync<List<DutyStatusLog>>(GET, typeof(DutyStatusLog)).Result;
            return logList;
        }

        public IEnumerable<DutyStatusLog> GetLogList(DutyStatusLogSearch logSearch)
        {
            var logList = api.CallAsync<List<DutyStatusLog>>(GET, typeof(DutyStatusLog), new { search = logSearch }).Result;
            return logList;
        }


        async Task<FeedResult<T>> MakeFeedCallAsync<T>(long? fromVersion)
            where T : Entity => await api.CallAsync<FeedResult<T>>("GetFeed", typeof(T), new { fromVersion });

        public async Task<FeedResult<DutyStatusLog>> GetFeedCall<DutyStatusLog>(long? fromVersion)
            where DutyStatusLog : Entity => await api.CallAsync<FeedResult<DutyStatusLog>>("GetFeed", typeof(DutyStatusLog), new { fromVersion });

    }
}
