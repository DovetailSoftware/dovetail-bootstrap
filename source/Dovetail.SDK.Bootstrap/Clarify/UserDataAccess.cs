using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FChoice.Foundation.Filters;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public class SDKUser
	{
		public string Login { get; set; }
		public string ImpersonatingLogin { get; set; }
		public string ProxyUserId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }

		public ITimeZone Timezone { get; set; }
		public IEnumerable<SDKUserQueue> Queues { get; set; }
		public string Workgroup { get; set; }
	}

	public interface IUserDataAccess
	{
		SDKUser GetUser(string username);
	}

	public class SDKUserQueue
	{
		public string Name { get; set; }
		public int DatabaseIdentifier { get; set; }
	}

	public class UserDataAccess : IUserDataAccess
	{
		private readonly IApplicationClarifySession _session;
		private readonly ILocaleCache _localeCache;
		private readonly IUserImpersonationService _userImpersonationService;
		private readonly ILogger _logger;

		public UserDataAccess(IApplicationClarifySession session, 
			ILocaleCache localeCache, 
			IUserImpersonationService userImpersonationService,
			ILogger logger)
		{
			_session = session;
			_localeCache = localeCache;
			_userImpersonationService = userImpersonationService;
			_logger = logger;
		}

		public SDKUser GetUser(string username)
		{
			var impersonatedLogin = _userImpersonationService.GetImpersonatedLoginFor(username);
			var login = username;
			string impersonateingLogin = null;
			if (impersonatedLogin != null)
			{
				_logger.LogDebug("Proxied user: setting {0} as the authenticated user being proxied by {1}", impersonatedLogin, username);
				login = impersonatedLogin;
				impersonateingLogin = username;
			}
		
			var dataSet = _session.CreateDataSet();
			var userGeneric = dataSet.CreateGeneric("user");
			userGeneric.Filter.AddFilter(FilterType.Equals("login_name", login));

			var employeeGeneric = userGeneric.TraverseWithFields("user2employee", "work_group", "first_name", "last_name");
			var siteGeneric = employeeGeneric.TraverseWithFields("supp_person_off2site");
			var addressGeneric = siteGeneric.TraverseWithFields("cust_primaddr2address");
			var timeZoneGeneric = addressGeneric.TraverseWithFields("address2time_zone", "name");

			var queueGeneric = userGeneric.Traverse("user_assigned2queue");
			queueGeneric.DataFields.Add("title");

			userGeneric.Query();

			if (userGeneric.Count < 1)
			{
				_logger.LogWarn("Could not find user {0}.", username);
				return null;
			}

			var employeeRow = employeeGeneric.DataRows().First();
			var queues = findQueues(queueGeneric);
			var timezone = findTimezone(timeZoneGeneric, username);

			return new SDKUser
			{
				FirstName = employeeRow.AsString("first_name"),
				LastName = employeeRow.AsString("last_name"),
				Workgroup = employeeRow.AsString("work_group"),
				Login = login,
				ImpersonatingLogin = impersonateingLogin,
				Queues = queues,
				Timezone = timezone
			};
		}

		private ITimeZone findTimezone(ClarifyGeneric timeZoneGeneric, string username)
		{
			if (timeZoneGeneric.Count < 1)
			{
				_logger.LogWarn("Could not find default timezone for user {0} using server default.", username);
				return _localeCache.ServerTimeZone;
			}

			var timezoneName = timeZoneGeneric.Rows[0].AsString("name");
			return _localeCache.TimeZones[timezoneName, false];
		}

		private static IEnumerable<SDKUserQueue> findQueues(ClarifyGeneric queueGeneric)
		{
			var queues = queueGeneric.DataRows().Select(row => new SDKUserQueue
			{
				DatabaseIdentifier = Convert.ToInt32(row.UniqueID),
				Name = row["title"].ToString()
			});
			return queues;
		}
	}
}