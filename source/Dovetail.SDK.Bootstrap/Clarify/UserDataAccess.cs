using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.DataObjects;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IUserDataAccess
    {
        FCTimeZone GetUserSiteTimezone(string username);
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

        public IEnumerable<User> UsersLoggingInToday(string username)
        {
            var dataSet = _session.CreateDataSet();
            var userGeneric = dataSet.CreateGenericWithFields("user", "login_name");
            userGeneric.Filter(f => f.And(f.Equals("login_name", username),
                                          f.After("last_login", DateTime.Now.AddDays(-1))
                                        ));
            userGeneric.Query();

            return userGeneric
                .DataRows()
                .Select(r => new User
                                 {
                                     ID = r.DatabaseIdentifier(),
                                     Login = r.AsString("login_name")
                                 });
        }
    }
}