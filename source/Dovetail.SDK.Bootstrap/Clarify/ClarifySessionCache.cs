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


		private readonly ConcurrentDictionary<string, IClarifySession> _agentSessionCacheByUsername;
		
        public ClarifySessionCache(IClarifyApplication clarifyApplication, ILogger logger, IUserClarifySessionConfigurator sessionConfigurator, Func<IUserSessionEndObserver> sessionEndObserver, Func<IUserSessionStartObserver> sessionStartObserver, DovetailDatabaseSettings settings)
        {
			_agentSessionCacheByUsername = new ConcurrentDictionary<string, IClarifySession>();
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
			
			_logger.LogDebug("Created and configured session {0} for agent {1}.".ToFormat(clarifySession.SessionID, username));

			return wrappedSession;
        }

	    public IClarifySession GetSession(string username)
		{
			return getSession(username);
		}

		public bool EjectSession(string username)
		{
			_logger.LogDebug("Ejecting session for {0}", username);
			IClarifySession value;
			var success = _agentSessionCacheByUsername.TryRemove(username, out value);
			if (success && !isApplicationUsername(username))
			{
				_sessionEndObserver().SessionExpired(value);
			}
			return success;
		}

		private bool isApplicationUsername(string username)
		{
			return username == _settings.ApplicationUsername;
		}

		public IClarifySession GetApplicationSession()
        {
			return getSession(_settings.ApplicationUsername);
        }

		private IClarifySession getSession(string username, int attempt = 1)
		{
			_logger.LogDebug("Getting session for user {0}. Attempt {1}.".ToFormat(username, attempt));

			if(attempt > MaximumAttempts)
			{
				throw new ApplicationException("Giving up getting session after {0} attempts for user {1}".ToFormat(attempt, username));
			}

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