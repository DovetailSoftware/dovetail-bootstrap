using System;
using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface ICurrentSDKUser
    {
    	string Username { get; }
        bool IsAuthenticated { get; }
        bool HasPermission(string permission);
        ITimeZone Timezone { get; }

        void SignOut();
        void SetUser(string username);
        void SetUser(IPrincipal principal);
    }

    public class CurrentSDKUser : ICurrentSDKUser
    {
        private IPrincipal _principal; 
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

        public bool HasPermission(string permission)
        {
            return _principal != null && _principal.IsInRole(permission);
        } 
        
        public void SetUser(IPrincipal principal)
        {
            _principal = principal;
            var username = principal.Identity.Name;

            var dataSet = _session.CreateDataSet();
            var userGeneric = dataSet.CreateGenericWithFields("user", "objid");
            userGeneric.Filter(f => f.Equals("login_name", username));

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

            if (timeZoneGeneric.Count < 1)
            {
                Timezone = _localeCache.ServerTimeZone;
                _logger.LogWarn("Could not determine a default timezone for user {0}. Defaulting to server timezone {1}.", username, Timezone.Name);
            }

            var timezoneName = timeZoneGeneric.Rows[0].AsString("name");
            Timezone = _localeCache.TimeZones[timezoneName, false];
            _logger.LogDebug("Timezone for user {0} set to {1}.", username, Timezone.Name);
        }

        public void SetUser(string username)
        {
            var principal = new DovetailPrincipal(new GenericIdentity(username), new string[0]);
            SetUser(principal);
        }

        public void SignOut()
        {
            IsAuthenticated = false;
            Username = _settings.ApplicationUsername;
        }
    }
}