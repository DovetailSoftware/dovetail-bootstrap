using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.DataObjects;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IUserDataAccess
    {
        FCTimeZone GetUserSiteTimezone(string username);
        IEnumerable<string> UserPermissions(string username);
    }

    public class UserDataAccess : IUserDataAccess
    {
        private readonly IApplicationClarifySession _session;
        private readonly ILocaleCache _localeCache;
        private readonly ILogger _logger;

        public UserDataAccess(IApplicationClarifySession session, ILocaleCache localeCache, ILogger logger)
        {
            _session = session;
            _localeCache = localeCache;
            _logger = logger;
        }

        public FCTimeZone GetUserSiteTimezone(string username)
        {
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

            if (timeZoneGeneric.Count < 1)
            {
                return null;
            }

            var timezoneName = timeZoneGeneric.Rows[0].AsString("name");
            var timezone = _localeCache.TimeZones[timezoneName, false];
            _logger.LogDebug("Timezone for user {0} set to {1}.", username, timezone.Name);

            return timezone;
        }

        public class User
        {
            public int ID { get; set; }
            public string Login { get; set; }
        }

        public IEnumerable<string> UserPermissions(string username)
        {
            var dataSet = _session.CreateDataSet();
            var userGeneric = dataSet.CreateGenericWithFields("user", "login_name");
            var privGeneric = userGeneric.TraverseWithFields("user_access2privclass");
            var cmdGeneric = privGeneric.TraverseWithFields("privclass2x_web_cmd", "x_name");

            userGeneric.Filter(f => f.Equals("login_name", username));
            userGeneric.Query();

            if (userGeneric.Count < 1) return new string[0];

            return cmdGeneric.DataRows().Select(r => r.AsString("x_name")).ToArray();
        }
    }
}