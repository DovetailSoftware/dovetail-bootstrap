using System.Collections.Generic;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionUsageReporter
	{
		IClarifySessionUsage GetUsage();
	}

	public class ClarifySessionUsageReporter : IClarifySessionUsageReporter
	{
		private readonly IClarifySessionCache _clarifySessionCache;
		private readonly IClarifyApplicationFactory _clarifyApplicationFactory;
		private readonly ILogger _logger;

		public ClarifySessionUsageReporter(IClarifySessionCache clarifySessionCache, IClarifyApplicationFactory clarifyApplicationFactory, ILogger logger)
		{
			_clarifySessionCache = clarifySessionCache;
			_clarifyApplicationFactory = clarifyApplicationFactory;
			_logger = logger;
		}

		public IClarifySessionUsage GetUsage()
		{
			var validSessions = new List<ClarifySessionUser>();
			var inValidSessions = new List<ClarifySessionUser>();

			var sessionsByUserDictionary = _clarifySessionCache.SessionsByUsername;

			var clarifyApplication = _clarifyApplicationFactory.Create();

			_logger.LogDebug("Building clarify session usage report from {0} items in the cache.".ToFormat(sessionsByUserDictionary.Count));

			foreach (var username in sessionsByUserDictionary.Keys)
			{
				var session = sessionsByUserDictionary[username];
				var sessionUser = new ClarifySessionUser { SessionId = session.Id, Username = session.UserName };
				if (clarifyApplication.IsSessionValid(session.Id))
					validSessions.Add(sessionUser);
				else
				{
					inValidSessions.Add(sessionUser);
					_logger.LogDebug("Ejecting invalid session for user {0} from the cache".ToFormat(username));
					_clarifySessionCache.EjectSession(username);
				}
			}

			_logger.LogDebug("Found {0} sessions and {1} invalid sessions (now ejected) in the cache.".ToFormat(validSessions.Count, inValidSessions.Count));

			return new ClarifySessionUsage(validSessions, inValidSessions);
		}
	}
}