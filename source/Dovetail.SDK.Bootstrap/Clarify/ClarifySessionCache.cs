using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Authentication;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
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

		public ClarifySessionCache(IClarifyApplication clarifyApplication, ILogger logger,
			IUserClarifySessionConfigurator sessionConfigurator, Func<IUserSessionEndObserver> sessionEndObserver,
			Func<IUserSessionStartObserver> sessionStartObserver, DovetailDatabaseSettings settings)
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

		public void RefreshSession(string username)
		{
			getSession(username).RefreshContext();
		}

		public int NumberOfActiveSessions
		{
			get
			{
				lock (_agentSessionCacheByUsername)
				{
					return _agentSessionCacheByUsername.Values.Count(_ => _clarifyApplication.IsSessionValid(_.Id));
				}
			}
		}

		public void CleanUpInvalidSessions()
		{
			_logger.LogDebug("Performing session cleanup");

			IEnumerable<IClarifySession> invalidSessions;
			lock (_agentSessionCacheByUsername)
			{
				invalidSessions = SessionsByUsername
					.Values
					.Where(_ => !_clarifyApplication.IsSessionValid(_.Id))
					.ToList();
			}

			foreach (var session in invalidSessions)
			{
				_logger.LogDebug("Ejecting inactive session {0} for user {1}.".ToFormat(session.Id, session.UserName));
				EjectSession(session.UserName);
			}

			var count = invalidSessions.Count();
			if (count != 0)
			{
				_logger.LogInfo("{0} sessions cleaned up", count);
			}
			else
			{
				_logger.LogDebug("No sessions to clean up");
			}
		}

		public void addSessionToCache(string username, IClarifySession session)
		{
			_agentSessionCacheByUsername.Add(username, session);
		}

		public void clear()
		{
			_agentSessionCacheByUsername.Clear();
		}

		public bool EjectSession(string username, bool isObserved = true)
		{
			using (_logger.Push("Ejecting session for {0}.".ToFormat(username)))
			{
				if (!_agentSessionCacheByUsername.ContainsKey(username))
				{
					_logger.LogDebug("No session was found for the user.");
					return false;
				}

				IClarifySession session;
				lock (_agentSessionCacheByUsername)
				{
					if (!_agentSessionCacheByUsername.ContainsKey(username))
					{
						_logger.LogDebug("Session was there when we started but someone else ejected it.");
						return false;
					}

					session = _agentSessionCacheByUsername[username];
					_agentSessionCacheByUsername.Remove(username);

					_logger.LogDebug("{0} sessions are now in the cache.", _agentSessionCacheByUsername.Count);
				}

				if (session == null) return true;

				if (isObserved)
				{
					_logger.LogDebug("Expiring session {0}.", session.Id);
					_sessionEndObserver().SessionExpired(session);
				}

				_logger.LogDebug("Closing session {0}.", session.Id);
				session.Close();
			}

			return true;
		}

		public IClarifySession GetApplicationSession(bool isConfigured = true)
		{
			_logger.LogDebug("Getting application {0}session.", isConfigured ? "configured " : "");
			return getSession(_settings.ApplicationUsername, isConfigured, false);
		}

		private IClarifySession getSession(string username, bool isConfigured = true, bool isObserved = true)
		{
			IClarifySession session;

			using (_logger.Push("Get session for {0}.".ToFormat(username)))
			{
				var success = _agentSessionCacheByUsername.TryGetValue(username, out session);
				if (success)
				{
					if (_clarifyApplication.IsSessionValid(session.Id) && session.As<IClarifySessionProxy>().Session.SessionData != null)
					{
						_logger.LogDebug("Found valid session in cache.");
						return session;
					}
					_logger.LogDebug("Ejecting invalid session.");
					EjectSession(username, isObserved);
				}

				ClarifySession clarifySession;
				lock (_agentSessionCacheByUsername)
				{
					if (_agentSessionCacheByUsername.ContainsKey(username))
					{
						_logger.LogDebug("Found session (within the lock). Assuming it is valid because it must be very recent.");
						return _agentSessionCacheByUsername[username];
					}

					//session = CreateSession(username, isConfigured, isObserved);
					_logger.LogDebug("Creating missing session.");

					clarifySession = _clarifyApplication.CreateSession(username, ClarifyLoginType.User);
				    clarifySession.SetNullStringsToEmpty = true;

                    session = wrapSession(clarifySession);

					_logger.LogInfo("Created session {0}.".ToFormat(clarifySession.SessionID));

					_agentSessionCacheByUsername.Add(username, session);

					_logger.LogDebug("{0} sessions are now in the cache.", _agentSessionCacheByUsername.Count);
				}

				visitSession(clarifySession, isConfigured, isObserved);
			}

			return session;
		}

		private void visitSession(ClarifySession session, bool isConfigured = true, bool isObserved = true)
		{
			if (isConfigured)
			{
				_sessionConfigurator.Configure(session);
				_logger.LogDebug("Configured created session.");
			}

			if (isObserved)
			{
				_sessionStartObserver().SessionStarted(wrapSession(session));
				_logger.LogDebug("Observed created session.");
			}
		}

		public IClarifySession CreateSession(string username, bool isConfigured = true, bool isObserved = true)
		{
			_logger.LogDebug("Creating missing session.");

			var clarifySession = _clarifyApplication.CreateSession(username, ClarifyLoginType.User);

			var wrappedSession = wrapSession(clarifySession);

			if (isConfigured)
			{
				_sessionConfigurator.Configure(clarifySession);
				_logger.LogDebug("Configured created session.");
			}

			if (isObserved)
			{
				_sessionStartObserver().SessionStarted(wrappedSession);
				_logger.LogDebug("Observed created session.");
			}

			_logger.LogInfo("Created session {0}.".ToFormat(clarifySession.SessionID));

			return wrappedSession;
		}

		private static IClarifySession wrapSession(ClarifySession session)
		{
			return new ClarifySessionWrapper(session);
		}
	}
}
