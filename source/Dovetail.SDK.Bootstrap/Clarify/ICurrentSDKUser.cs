using System;
using System.Collections.Generic;
using System.Security.Principal;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface ICurrentSDKUser
	{
		string Username { get; }
		string ProxyUsername { get; }
		string Fullname { get; }
		bool IsAuthenticated { get; }
		bool HasPermission(string permission);
		ITimeZone Timezone { get; }
		IEnumerable<SDKUserQueue> Queues { get; }
		string Workgroup { get; }

		void SignOut();
		void SetUser(IPrincipal principal);
		void SetTimezone(ITimeZone timezone);
	}

	public class CurrentSDKUser : ICurrentSDKUser
	{
		private IPrincipal _principal;
		private readonly DovetailDatabaseSettings _settings;
		private readonly IUserDataAccess _userDataAccess;
		private readonly ILogger _logger;
		private readonly ILocaleCache _localeCache;
		private Lazy<SDKUser> _user;
		private Lazy<ITimeZone> _timezone;
		private string _authenticatedUserName;

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

		public string ProxyUsername
		{
			get { return _user.Value.ProxyLogin; }
		}

		public ITimeZone Timezone
		{
			get
			{
				if (!IsAuthenticated)
					return _localeCache.ServerTimeZone;

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
					return new SDKUserQueue[0];

				return _user.Value.Queues;
			}
		}

		public string Workgroup
		{
			get
			{
				if (!IsAuthenticated) return "";

				return _user.Value.Workgroup;
			}
		}

		public CurrentSDKUser(DovetailDatabaseSettings settings, ILocaleCache localeCache, IUserDataAccess userDataAccess,
			ILogger logger)
		{
			_settings = settings;
			_userDataAccess = userDataAccess;
			_logger = logger;
			_localeCache = localeCache;

			//set up defaults
			SignOut();

			_user = new Lazy<SDKUser>(GetUser);
			_timezone = new Lazy<ITimeZone>(() => _user.Value.Timezone);
		}

		public bool HasPermission(string permission)
		{
			return _principal != null && _principal.IsInRole(permission);
		}

		public void SetUser(IPrincipal principal)
		{
			_principal = principal;

			_authenticatedUserName = _principal.Identity.Name;
			_user = new Lazy<SDKUser>(GetUser);

			_logger.LogDebug("Setting the current user to be {0}", _authenticatedUserName);

			IsAuthenticated = true;
		}

		public void SignOut()
		{
			IsAuthenticated = false;

			//when no user is authenticated the application user is used
			_authenticatedUserName = _settings.ApplicationUsername;
		}

		private SDKUser GetUser()
		{
			return _userDataAccess.GetUser(_authenticatedUserName);
		}
	}
}