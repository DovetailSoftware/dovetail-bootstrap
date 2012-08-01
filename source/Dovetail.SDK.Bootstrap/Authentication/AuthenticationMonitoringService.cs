using System;
using System.Linq;
using System.Net.NetworkInformation;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using Dovetail.SDK.Bootstrap.Configuration;
using FChoice.Foundation;
using FChoice.Foundation.Schema;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IUserSessionEndObserver
	{
		void SessionExpired(IClarifySession session);
	}

	public class UserSessionEndObserver : IUserSessionEndObserver
	{
		private readonly Func<IApplicationClarifySession> _session;
		private readonly ISchemaCache _schemaCache;
		private readonly ILogger _logger;

		public UserSessionEndObserver(Func<IApplicationClarifySession> session, ISchemaCache schemaCache, ILogger logger)
		{
			_session = session;
			_schemaCache = schemaCache;
			_logger = logger;
		}

		public void SessionExpired(IClarifySession session)
		{
			var id = session.Id;
			if (!_schemaCache.IsValidTable("fc_login_monitor"))
			{
				_logger.LogDebug("Unable to log session expiration. No fc_login_monitor table is present.");
				return;
			}

			var dataSet = _session().CreateDataSet();
			var monitorGeneric = dataSet.CreateGenericWithFields("fc_login_monitor");
			monitorGeneric.Filter(f => f.Equals("fcsessionid", id.ToString()));
			monitorGeneric.Query();

			if(monitorGeneric.Count < 1)
			{
				_logger.LogDebug("Could not find expired session {0} row in login monitor", id);
				return;
			}

			var monitorRow = monitorGeneric.DataRows().First();
			monitorRow["logout_time"] = FCGeneric.NOW_DATE;
			monitorRow.Update();

			_logger.LogDebug("Set logout time for session {0} in login monitor.", id);
		}
	}

	public interface IUserSessionStartObserver
	{
		void SessionStarted(IClarifySession session);
	}

	public class UserSessionStartObserver : IUserSessionStartObserver
	{
		private readonly WebsiteSettings _settings;
		private readonly ISchemaCache _schemaCache;
		private readonly IWebApplicationUrl _applicationUrl;
		private readonly IClarifySessionUsageReporter _sessionUsageReporter;
		private readonly ILogger _logger;

		public UserSessionStartObserver(WebsiteSettings settings, ISchemaCache schemaCache, IWebApplicationUrl applicationUrl, IClarifySessionUsageReporter sessionUsageReporter, ILogger logger)
		{
			_settings = settings;
			_schemaCache = schemaCache;
			_applicationUrl = applicationUrl;
			_sessionUsageReporter = sessionUsageReporter;
			_logger = logger;
		}

		public static string GetLocalhostFqdn()
		{
			var ipProperties = IPGlobalProperties.GetIPGlobalProperties();

			var hostName = ipProperties.HostName;
			var domain = ipProperties.DomainName;
			
			return domain.IsEmpty() ? hostName : string.Format("{0}.{1}", hostName, domain);
		}

		public void SessionStarted(IClarifySession session)
		{
			if (!_schemaCache.IsValidTable("fc_login_monitor"))
			{
				_logger.LogDebug("Unable to log user authentication. No fc_login_monitor table is present.");
				return;
			}

			var host = GetLocalhostFqdn();

			_logger.LogDebug("Logging application {0} authentication for user {1} on host {2} with session id {3}.", _settings.ApplicationName, session.UserName, host, session.Id);

			var dataSet = session.CreateDataSet();
			
			var monitorGeneric = dataSet.CreateGeneric("fc_login_monitor");
			var monitorRow = monitorGeneric.AddNew();
			monitorRow["application"] = _settings.ApplicationName;
			monitorRow["login_time"] = FCGeneric.NOW_DATE;
			monitorRow["logout_time"] = FCGeneric.MIN_DATE;
			monitorRow["login_name"] = session.UserName;
			monitorRow["fcsessionid"] = session.Id.ToString();
			monitorRow["num_sessions"] = _sessionUsageReporter.GetActiveSessionCount();

			if(_schemaCache.IsValidField("fc_login_monitor", "server"))
			{
				monitorRow["server"] = host;
			}

			monitorRow["comments"] = _applicationUrl.Url;

			monitorRow.RelateByID(session.SessionUserID, "fc_login_monitor2user");
			monitorRow.Update();
		}
	}
}