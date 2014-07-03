using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Authentication;
using FChoice.Common.Data;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionCache
    {
		IClarifySession GetApplicationSession();
		IClarifySession GetSession(string username);
		bool EjectSession(string username);
		IDictionary<string, IClarifySession> SessionsByUsername { get; }
    }

	public class ClarifySessionCache : IClarifySessionCache
	{
		private static readonly Dictionary<string, IClarifySession> _agentSessionCacheByUsername;

		private readonly IClarifyApplication _clarifyApplication;
		private readonly ILogger _logger;
		private readonly IUserClarifySessionConfigurator _sessionConfigurator;
		private readonly Func<IUserSessionEndObserver> _sessionEndObserver;
		private readonly Func<IUserSessionStartObserver> _sessionStartObserver;
		private readonly DovetailDatabaseSettings _settings;

		static ClarifySessionCache()
		{
			_agentSessionCacheByUsername = new Dictionary<string, IClarifySession>();
		}

		public ClarifySessionCache(IClarifyApplication clarifyApplication, ILogger logger, IUserClarifySessionConfigurator sessionConfigurator, Func<IUserSessionEndObserver> sessionEndObserver, Func<IUserSessionStartObserver> sessionStartObserver, DovetailDatabaseSettings settings)
		{
			_clarifyApplication = clarifyApplication;
			_logger = logger;
			_sessionConfigurator = sessionConfigurator;
			_sessionEndObserver = sessionEndObserver;
			_sessionStartObserver = sessionStartObserver;
			_settings = settings;
		}

		public IDictionary<string, IClarifySession> SessionsByUsername
		{
			get { return new Dictionary<string, IClarifySession>(_agentSessionCacheByUsername); }
		}

		public IClarifySession GetSession(string username)
		{
			return getSession(username);
		}

		public void addSessionToCache(string username, IClarifySession session)
		{
			_agentSessionCacheByUsername.Add(username, session);
		}

		public void clear()
		{
			_agentSessionCacheByUsername.Clear();
		}

		public bool EjectSession(string username)
		{
			var isApplicationUser = isApplicationUsername(username);

			using (_logger.Push("Ejecting session for {0}.".ToFormat(username)))
			{
				if (!_agentSessionCacheByUsername.ContainsKey(username))
				{
					_logger.LogDebug("No session was found for the user.");
					return false;
				}

				lock (_agentSessionCacheByUsername)
				{
					if (!_agentSessionCacheByUsername.ContainsKey(username))
					{
						_logger.LogDebug("Session was there when we started but someone else ejected it.");
						return false;
					}

					var session = _agentSessionCacheByUsername[username];
					_agentSessionCacheByUsername.Remove(username);

					if (!isApplicationUser)
					{
						_logger.LogDebug("Expiring session {0}.", session.Id);
						_sessionEndObserver().SessionExpired(session);
					}

					_logger.LogDebug("Closing session {0}.", session.Id);
					session.Close();

					_logger.LogDebug("{0} sessions are now in the cache.", _agentSessionCacheByUsername.Count);
				}
			}

			return true;
		}

		private bool isApplicationUsername(string username)
		{
			return username == _settings.ApplicationUsername;
		}

		public IClarifySession GetApplicationSession()
		{
			_logger.LogDebug("Getting application session.");
			return getSession(_settings.ApplicationUsername);
		}

		private IClarifySession getSession(string username)
		{
			IClarifySession session;

			using (_logger.Push("Get session for {0}.".ToFormat(username)))
			{
				var success = _agentSessionCacheByUsername.TryGetValue(username, out session);
				if (success)
				{
					if (_clarifyApplication.IsSessionValid(session.Id))
					{
						_logger.LogDebug("Found valid session in cache.");
						return session;
					}
					_logger.LogDebug("Ejecting invalid session.");
					EjectSession(username);
				}

				lock (_agentSessionCacheByUsername)
				{
					if (_agentSessionCacheByUsername.ContainsKey(username))
					{
						_logger.LogDebug("Found session (within the lock). Assuming it is valid because it must be very recent.");
						return _agentSessionCacheByUsername[username];
					}

					session = CreateSession(username);
					_agentSessionCacheByUsername.Add(username, session);

					_logger.LogDebug("{0} sessions are now in the cache.", _agentSessionCacheByUsername.Count);
				}
			}

			return session;
		}

		public IClarifySession CreateSession(string username)
		{
			_logger.LogDebug("Creating missing session.");

			string actualUserName = username;
			int proxyUserId = -1;
			//if the desired user currently proxying another user
			var sqlHelper = new SqlHelper("SELECT u.login_name, u.objid FROM table_user u, table_user p WHERE p.user2proxy_user = u.objid AND p.login_name = {0}");
			sqlHelper.Parameters.Add("login", username);

			using (var dr = sqlHelper.ExecuteReader())
			{
				if (dr.Read())
				{
					actualUserName = dr.GetString(0);
					proxyUserId = dr.GetInt32(1);
				}
			}

			var clarifySession = _clarifyApplication.CreateSession(actualUserName, ClarifyLoginType.User);

			//save the original requested username as the proxy user in the session state
			if (actualUserName != username)
			{
				clarifySession["proxy.login_name"] = username;
				clarifySession["proxy.userid"] = proxyUserId;
			}

			var wrappedSession = wrapSession(clarifySession);

			_sessionConfigurator.Configure(clarifySession);
			_logger.LogDebug("Configured created session.");

			if (!isApplicationUsername(actualUserName))
			{
				_sessionStartObserver().SessionStarted(wrappedSession);
			}

			_logger.LogInfo("Created session {0}.".ToFormat(clarifySession.SessionID));

			return wrappedSession;
		}

		private IClarifySession wrapSession(ClarifySession session)
		{
			return new ClarifySessionWrapper(session);
		}
	}
}