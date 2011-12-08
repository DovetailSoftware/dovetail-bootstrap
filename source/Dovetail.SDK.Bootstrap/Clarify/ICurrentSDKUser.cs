using System;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface ICurrentSDKUser
    {
    	string Username { get; }
        ITimeZone Timezone { get; }

        void SetUserName(string username);
    }

    public class CurrentSDKUser : ICurrentSDKUser
    {
        private readonly IApplicationClarifySession _session;
        private readonly ILogger _logger;
        private readonly ILocaleCache _localeCache;

        public string Username { get; set; }
        public ITimeZone Timezone { get; set; }

        public CurrentSDKUser(IApplicationClarifySession session, DovetailDatabaseSettings settings, ILocaleCache localeCache, ILogger logger)
        {
            //defaults to application user and the server timezone
            Username = settings.ApplicationUsername;
            Timezone = localeCache.ServerTimeZone;

            _session = session;
            _logger = logger;
            _localeCache = localeCache;
        }

        public void SetUserName(string username)
        {
            var dataSet = _session.CreateDataSet();
            var userGeneric = dataSet.CreateGeneric("user");
            userGeneric.Filter(f => f.Equals("login_name", username));
            userGeneric.DataFields.Add("objid");

            var employeeGeneric = userGeneric.TraverseWithFields("user2employee");
            var siteGeneric = employeeGeneric.TraverseWithFields("supp_person_off2site");
            var addressGeneric = siteGeneric.TraverseWithFields("cust_primaddr2address");
            var timeZoneGeneric = addressGeneric.TraverseWithFields("address2time_zone", "name");

            dataSet.Query(userGeneric);
            
            if (userGeneric.Count < 1)
            {
                throw new ApplicationException("User {0} does not exist.".ToFormat(username));
            }

            Username = username;

            if(timeZoneGeneric.Count < 1)
            {
                Timezone = _localeCache.ServerTimeZone;
                _logger.LogWarn("Could not determine a default timezone for user {0}. Defaulting to server timezone {1}.", username, Timezone);
            }

            var timezoneName = timeZoneGeneric.Rows[0].AsString("name");
            Timezone = _localeCache.TimeZones[timezoneName, false];
            _logger.LogDebug("Timezone for user {0} set to {1}.", username, Timezone);
        }
    }
}