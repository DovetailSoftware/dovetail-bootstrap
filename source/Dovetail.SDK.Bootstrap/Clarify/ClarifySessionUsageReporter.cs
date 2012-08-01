using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionUsageReporter
	{
		IClarifySessionUsage GetUsage();
		int GetActiveSessionCount();
	}

	public class ClarifySessionUsageReporter : IClarifySessionUsageReporter
	{
		private readonly IClarifySessionCache _clarifySessionCache;
		private readonly IClarifyApplication _clarifyApplication;
		private readonly ILogger _logger;

		public ClarifySessionUsageReporter(IClarifySessionCache clarifySessionCache, IClarifyApplication clarifyApplication, ILogger logger)
		{
			_clarifySessionCache = clarifySessionCache;
			_clarifyApplication = clarifyApplication;
			_logger = logger;
		}

		public IClarifySessionUsage GetUsage()
		{
			var validSessions = new List<ClarifySessionUser>();
			var inValidSessions = new List<ClarifySessionUser>();

			var sessionsByUserDictionary = _clarifySessionCache.SessionsByUsername;

			_logger.LogDebug("Building clarify session usage report from {0} items in the cache.".ToFormat(sessionsByUserDictionary.Count));

			foreach (var username in sessionsByUserDictionary.Keys)
			{
				var session = sessionsByUserDictionary[username];
				var sessionUser = new ClarifySessionUser { SessionId = session.Id, Username = session.UserName };
				if (_clarifyApplication.IsSessionValid(session.Id))
					validSessions.Add(sessionUser);
				else
				{
					inValidSessions.Add(sessionUser);
					invalidateSession(sessionUser);
				}
			}

			_logger.LogDebug("Found {0} sessions and {1} invalid sessions (now ejected) in the cache.".ToFormat(validSessions.Count, inValidSessions.Count));

			return new ClarifySessionUsage(validSessions, inValidSessions);
		}

		private void invalidateSession(ClarifySessionUser sessionUser)
		{
			_logger.LogDebug("Invalidating session for user {0}.".ToFormat(sessionUser.Username));
			_clarifySessionCache.EjectSession(sessionUser.Username);
		}

		public int GetActiveSessionCount()
		{
			var sessionsByUsername = _clarifySessionCache.SessionsByUsername;
			
			return sessionsByUsername.Keys.Count(k=>
				{
					var id = sessionsByUsername[k].Id;
					return _clarifyApplication.IsSessionValid(id);
				});
		}
	}
}