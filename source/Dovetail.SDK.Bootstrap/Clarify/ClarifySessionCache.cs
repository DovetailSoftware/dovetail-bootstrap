using System;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Authentication;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionCache
	{
		IClarifySession GetApplicationSession(bool isConfigured = true);
		IClarifySession GetSession(string username);
		bool EjectSession(string username, bool isObserved = true);
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

				lock (_agentSessionCacheByUsername)
				{
					if (!_agentSessionCacheByUsername.ContainsKey(username))
					{
						_logger.LogDebug("Session was there when we started but someone else ejected it.");
						return false;
					}

					var session = _agentSessionCacheByUsername[username];
					_agentSessionCacheByUsername.Remove(username);

					if (isObserved)
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

		public IClarifySession GetApplicationSession(bool isConfigured = true)
		{
			_logger.LogDebug("Getting application {0}session.", isConfigured ? "configured ":"");
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
					if (_clarifyApplication.IsSessionValid(session.Id))
					{
						_logger.LogDebug("Found valid session in cache.");
						return session;
					}
					_logger.LogDebug("Ejecting invalid session.");
					EjectSession(username, isObserved);
				}

				lock (_agentSessionCacheByUsername)
				{
					if (_agentSessionCacheByUsername.ContainsKey(username))
					{
						_logger.LogDebug("Found session (within the lock). Assuming it is valid because it must be very recent.");
						return _agentSessionCacheByUsername[username];
					}

					session = CreateSession(username, isConfigured, isObserved);
					_agentSessionCacheByUsername.Add(username, session);

					_logger.LogDebug("{0} sessions are now in the cache.", _agentSessionCacheByUsername.Count);
				}
			}

			return session;
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