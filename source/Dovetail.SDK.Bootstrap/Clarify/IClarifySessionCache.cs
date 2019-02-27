using System.Collections.Generic;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionCache
	{
		IClarifySession GetApplicationSession(bool isConfigured = true);
		IClarifySession GetSession(string username);
		bool EjectSession(string username, bool isObserved = true);
		IDictionary<string, IClarifySession> SessionsByUsername { get; }
		void RefreshSession(string username);

		int NumberOfActiveSessions { get; }
		void CleanUpInvalidSessions();
	}
}
