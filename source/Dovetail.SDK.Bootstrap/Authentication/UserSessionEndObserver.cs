using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation;
using FChoice.Foundation.Schema;

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
}