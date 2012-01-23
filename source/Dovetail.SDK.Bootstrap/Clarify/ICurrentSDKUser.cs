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
        bool IsAuthenticated { get; }

        void SetUserName(string username);
        void SignOut();
    }

    public class CurrentSDKUser : ICurrentSDKUser
    {
        private readonly IApplicationClarifySession _session;
        private readonly DovetailDatabaseSettings _settings;
        private readonly ILogger _logger;
        private readonly ILocaleCache _localeCache;
        public bool IsAuthenticated { get; private set; }

        public string Username { get; set; }
        public ITimeZone Timezone { get; set; }

        public CurrentSDKUser(IApplicationClarifySession session, DovetailDatabaseSettings settings, ILocaleCache localeCache, ILogger logger)
        {
            //defaults to application user and the server timezone
            Timezone = localeCache.ServerTimeZone;

            _session = session;
            _settings = settings;
            _logger = logger;
            _localeCache = localeCache;

            SignOut();
        }

        public void SetUserName(string username)
        {
            //verify user exists

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
            IsAuthenticated = true;

            //get user timezone based on their site's primary address

            if(timeZoneGeneric.Count < 1)
            {
                Timezone = _localeCache.ServerTimeZone;
                _logger.LogWarn("Could not determine a default timezone for user {0}. Defaulting to server timezone {1}.", username, Timezone.Name);
            }

            var timezoneName = timeZoneGeneric.Rows[0].AsString("name");
            Timezone = _localeCache.TimeZones[timezoneName, false];
            _logger.LogDebug("Timezone for user {0} set to {1}.", username, Timezone.Name);
        }

        public void SignOut()
        {
            IsAuthenticated = false;
            Username = _settings.ApplicationUsername;
        }
    }
}