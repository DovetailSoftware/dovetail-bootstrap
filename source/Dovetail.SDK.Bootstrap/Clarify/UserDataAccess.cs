using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.DataObjects;
using FChoice.Foundation.Filters;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IUserDataAccess
    {
        FCTimeZone GetUserSiteTimezone(string username);
        IEnumerable<UserQueue> GetQueueMemberships(string userName);
    }

    public class UserQueue
    {
        public string Name { get; set; }
        public int DatabaseIdentifier { get; set; }
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

        public IEnumerable<UserQueue> GetQueueMemberships(string userName)
        {
            var dataSet = _session.CreateDataSet();
            var userGeneric = dataSet.CreateGeneric("user");
            userGeneric.Filter.AddFilter(FilterType.Equals("login_name", userName));

            var queueGeneric = userGeneric.Traverse("user_assigned2queue");
            queueGeneric.DataFields.Add("title");

            userGeneric.Query();

            if (userGeneric.Count < 1)
            {
                _logger.LogWarn("Could not find queue membership for employee {0} that does not exist.", userName);
                return new UserQueue[0];
            }

            return queueGeneric.DataRows().Select(row => new UserQueue
            {
                DatabaseIdentifier = Convert.ToInt32(row.UniqueID),
                Name = row["title"].ToString()
            });
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
    }
}