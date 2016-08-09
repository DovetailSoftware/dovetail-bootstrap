using System;
using System.Collections.Generic;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface ICurrentSDKUser
	{
		string Username { get; }
		string ImpersonatingUsername { get; }
		string Fullname { get; }
		bool IsAuthenticated { get; }
		bool HasPermission(string permission);
		ITimeZone Timezone { get; }
		IEnumerable<SDKUserQueue> Queues { get; }
		string Workgroup { get; }
		string PrivClass { get; }

		void SignOut();
		void SetUser(string clarifyLoginName);
		void SetTimezone(ITimeZone timezone);
	}

	public class CurrentSDKUser : ICurrentSDKUser
	{
		private readonly DovetailDatabaseSettings _settings;
		private readonly IUserDataAccess _userDataAccess;
		private readonly IClarifySessionCache _sessionCache;
		private readonly ILogger _logger;
		private readonly ILocaleCache _localeCache;
		private Lazy<SDKUser> _user;
		private Lazy<ITimeZone> _timezone;
		private Lazy<HashSet<string>> _permissionsByName;

		public string Fullname
		{
			get
			{
				if (!IsAuthenticated) return "";

				var user = _user.Value;
				return user.FirstName + " " + user.LastName;
			}
		}

		public bool IsAuthenticated { get; private set; }

		public string Username
		{
			get { return _user.Value.Login; }
		}

		public string ImpersonatingUsername
		{
			get { return _user.Value.ImpersonatingLogin; }
		}

		public ITimeZone Timezone
		{
			get
			{
				if (!IsAuthenticated)
				{
					return _localeCache.ServerTimeZone;
				}

				return _timezone.Value;
			}
		}

		public void SetTimezone(ITimeZone timezone)
		{
			_timezone = new Lazy<ITimeZone>(() => timezone);
		}

		public IEnumerable<SDKUserQueue> Queues
		{
			get
			{
				if (!IsAuthenticated)
				{
					return new SDKUserQueue[0];
				}

				return _user.Value.Queues;
			}
		}

		public string Workgroup
		{
			get
			{
				if (!IsAuthenticated)
				{
					return "";
				}

				return _user.Value.Workgroup;
			}
		}

		public string PrivClass
		{
			get
			{
				if (!IsAuthenticated)
				{
					return "";
				}

				return _user.Value.PrivClass;
			}
		}

		public CurrentSDKUser(DovetailDatabaseSettings settings,
			ILocaleCache localeCache,
			IUserDataAccess userDataAccess,
			IClarifySessionCache sessionCache,
			ILogger logger)
		{
			_settings = settings;
			_userDataAccess = userDataAccess;
			_sessionCache = sessionCache;
			_logger = logger;
			_localeCache = localeCache;

			//set up defaults
			SignOut();
		}

		public bool HasPermission(string permission)
		{
			return _permissionsByName.Value.Contains(permission);
		}

		public void SetUser(string clarifyLoginName)
		{
			_logger.LogDebug("CurrentSDK user set via principal to {0}.".ToFormat(clarifyLoginName));
			changeUser(clarifyLoginName);

			IsAuthenticated = true;
		}

		public void SignOut()
		{
			_logger.LogDebug("Signing out currentSDK user.");

			IsAuthenticated = false;

			//when no user is authenticated the application user is used
			changeUser(_settings.ApplicationUsername);
		}

		public void changeUser(string login)
		{
			_logger.LogDebug("Changing the current SDK user to be {0}.", login);
			_user = new Lazy<SDKUser>(() => GetUser(login));
			_permissionsByName = new Lazy<HashSet<string>>(GetSessionPermissions);
			_timezone = new Lazy<ITimeZone>(() => _user.Value.Timezone);
		}

		private SDKUser GetUser(string login)
		{
			return _userDataAccess.GetUser(login);
		}

		private HashSet<string> GetSessionPermissions()
		{
			var session = _sessionCache.GetSession(_user.Value.Login);
			var set = new HashSet<string>();
			set.UnionWith(session.Permissions);
			_logger.LogDebug("Permission set for {0} setup with {1} permissions.".ToFormat(_user.Value.Login, set.Count));
			return set;
		}
	}
}
