using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public class ClarifySessionUsageReporter
	{
		private readonly IClarifySessionCache _clarifySessionCache;
		private readonly IClarifyApplicationFactory _clarifyApplicationFactory;

		public ClarifySessionUsageReporter(IClarifySessionCache clarifySessionCache, IClarifyApplicationFactory clarifyApplicationFactory)
		{
			_clarifySessionCache = clarifySessionCache;
			_clarifyApplicationFactory = clarifyApplicationFactory;
		}

		public IClarifySessionUsage GetUsage()
		{
			var validSessions = new List<ClarifySessionUser>();
			var inValidSessions = new List<ClarifySessionUser>();

			var sessionsByUserDictionary = _clarifySessionCache.SessionsByUsername;

			var clarifyApplication = _clarifyApplicationFactory.Create();

			foreach (var username in sessionsByUserDictionary.Keys)
			{
				var session = sessionsByUserDictionary[username];
				var sessionUser = new ClarifySessionUser { SessionId = session.Id, Username = session.UserName };
				if (clarifyApplication.IsSessionValid(session.Id))
					validSessions.Add(sessionUser);
				else
				{
					inValidSessions.Add(sessionUser);
					_clarifySessionCache.EjectSession(username);
				}
			}

			return new ClarifySessionUsage(validSessions, inValidSessions);
		}
	}
}