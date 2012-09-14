using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dovetail.SDK.Bootstrap.Authentication;
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
		public const int MaximumAttempts = 3;
		private readonly IClarifyApplication _clarifyApplication; 
		private readonly ILogger _logger;
    	private readonly IUserClarifySessionConfigurator _sessionConfigurator;
		private readonly Func<IUserSessionEndObserver> _sessionEndObserver;
		private readonly Func<IUserSessionStartObserver> _sessionStartObserver;
		private readonly DovetailDatabaseSettings _settings;

		private static readonly ConcurrentDictionary<string, IClarifySession> _agentSessionCacheByUsername = new ConcurrentDictionary<string, IClarifySession>();
		
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

		private IClarifySession onAgentMissing(string username)
        {
            _logger.LogDebug("Creating missing session for agent {0}.".ToFormat(username));

			var clarifySession = _clarifyApplication.CreateSession(username, ClarifyLoginType.User);
			var wrappedSession = wrapSession(clarifySession);

			if (!isApplicationUsername(username))
			{
				_sessionConfigurator.Configure(clarifySession);
				_sessionStartObserver().SessionStarted(wrappedSession);
			}
			
			_logger.LogInfo("Created and configured session {0} for agent {1}.".ToFormat(clarifySession.SessionID, username));

			return wrappedSession;
        }

	    public IClarifySession GetSession(string username)
		{
			return getSession(username);
		}

		public bool addSessionToCache(string username, IClarifySession session)
		{
			return _agentSessionCacheByUsername.TryAdd(username, session);
		}

		public bool EjectSession(string username)
		{
			_logger.LogDebug("Ejecting session for {0}.", username);
			IClarifySession ejectingSession;
			var success = _agentSessionCacheByUsername.TryRemove(username, out ejectingSession);
			var isApplicationUser = isApplicationUsername(username);
			if (!success)
			{
				_logger.LogDebug("Session could not be ejected for user {0} as it was not found in the cache.", username);
				return false;
			}

			if (!isApplicationUser)
			{
				_logger.LogDebug("Expiring session {0} for user {1}.", ejectingSession.Id, username);
				_sessionEndObserver().SessionExpired(ejectingSession);
			}

			_logger.LogDebug("Closing ejected session {0} for user {1}", ejectingSession.Id, username);
			ejectingSession.Close();

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

		private IClarifySession getSession(string username, int attempt = 1)
		{
			if(attempt > MaximumAttempts)
			{
				throw new ApplicationException("Giving up getting session after {0} attempts for user {1}".ToFormat(attempt, username));
			}

			_logger.LogDebug("Getting session for user {0}. Attempt {1}.".ToFormat(username, attempt));

			var session = _agentSessionCacheByUsername.GetOrAdd(username, onAgentMissing);

			if (_clarifyApplication.IsSessionValid(session.Id))
		    {
			    return session;
		    }

			EjectSession(username);
			attempt += 1;
		    
			return getSession(username, attempt);
	    }

		private IClarifySession wrapSession(ClarifySession session)
        {
            return new ClarifySessionWrapper(session);
        }
    }
}