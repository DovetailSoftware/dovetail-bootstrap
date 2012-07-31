using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		private readonly IClarifyApplicationFactory _clarifyApplicationFactory;
        private readonly ILogger _logger;
    	private readonly IUserClarifySessionConfigurator _sessionConfigurator;
		private readonly DovetailDatabaseSettings _settings;

		private IClarifyApplication _clarifyApplication;
		private readonly ConcurrentDictionary<string, IClarifySession> _agentSessionCacheByUsername;
		
        public ClarifySessionCache(IClarifyApplicationFactory clarifyApplicationFactory, ILogger logger, IUserClarifySessionConfigurator sessionConfigurator, DovetailDatabaseSettings settings)
        {
			_agentSessionCacheByUsername = new ConcurrentDictionary<string, IClarifySession>();
	        _clarifyApplicationFactory = clarifyApplicationFactory;
            _logger = logger;
        	_sessionConfigurator = sessionConfigurator;
	        _settings = settings;
        }

        public IClarifyApplication ClarifyApplication
        {
            get { return _clarifyApplication ?? (_clarifyApplication = _clarifyApplicationFactory.Create()); }
        }

		public IDictionary<string, IClarifySession> SessionsByUsername
		{
			get { return new Dictionary<string, IClarifySession>(_agentSessionCacheByUsername); }
		}

		private IClarifySession onAgentMissing(string username)
        {
            _logger.LogDebug("Creating missing session for agent {0}.".ToFormat(username));

			var clarifySession = ClarifyApplication.CreateSession(username, ClarifyLoginType.User);

			if (username != _settings.ApplicationUsername)
			{
				_sessionConfigurator.Configure(clarifySession);
			}
			
			_logger.LogDebug("Created and configured session {0} for agent {1}.".ToFormat(clarifySession.SessionID, username));

            return wrapSession(clarifySession);
        }

	    public IClarifySession GetSession(string username)
		{
			return getSession(username);
		}

		public bool EjectSession(string username)
		{
			IClarifySession value;
			return _agentSessionCacheByUsername.TryRemove(username, out value);
		}

		public IClarifySession GetApplicationSession()
        {
			return getSession(_settings.ApplicationUsername);
        }

		private IClarifySession getSession(string username, int attempt = 0)
		{
			if(attempt > MaximumAttempts)
			{
				throw new ApplicationException("Giving up gettig session after {0} attempts for user {1}".ToFormat(attempt, username));
			}

			var session = _agentSessionCacheByUsername.GetOrAdd(username, onAgentMissing);

		    if (ClarifyApplication.IsSessionValid(session.Id))
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