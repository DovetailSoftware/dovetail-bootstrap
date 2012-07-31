using System.Collections.Generic;
using FChoice.Foundation.Clarify;
using FubuCore;
using FubuCore.Util;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionCache
    {
		IApplicationClarifySession GetApplicationSession();
		IClarifySession GetSession(string username);
		void EjectSession(string username);
		IDictionary<string, IApplicationClarifySession> SessionsByName { get; }
    }

	public class ClarifySessionCache : IClarifySessionCache
    {
	    private readonly IClarifyApplicationFactory _clarifyApplicationFactory;
        private readonly ILogger _logger;
    	private readonly IUserClarifySessionConfigurator _sessionConfigurator;
		private readonly DovetailDatabaseSettings _settings;

		private ClarifyApplication _clarifyApplication;
		private readonly Cache<string, IApplicationClarifySession> _agentSessionCacheByUsername;
		
        public ClarifySessionCache(IClarifyApplicationFactory clarifyApplicationFactory, ILogger logger, IUserClarifySessionConfigurator sessionConfigurator, DovetailDatabaseSettings settings)
        {
	        _agentSessionCacheByUsername = new Cache<string, IApplicationClarifySession> {OnMissing = onAgentMissing};
	        _clarifyApplicationFactory = clarifyApplicationFactory;
            _logger = logger;
        	_sessionConfigurator = sessionConfigurator;
	        _settings = settings;
        }

        public ClarifyApplication ClarifyApplication
        {
            get { return _clarifyApplication ?? (_clarifyApplication = _clarifyApplicationFactory.Create()); }
        }

		public IDictionary<string, IApplicationClarifySession> SessionsByName
		{
			get { return _agentSessionCacheByUsername.ToDictionary(); }
		}

		private IApplicationClarifySession onAgentMissing(string username)
        {
            _logger.LogDebug("Creating missing session for agent {0}.".ToFormat(username));

			var clarifySession = (username == _settings.ApplicationUsername) ? ClarifyApplication.CreateSession(_settings.ApplicationUsername, ClarifyLoginType.User) : ClarifyApplication.CreateSession(username, ClarifyLoginType.User);

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

		public void EjectSession(string username)
		{
			if(_agentSessionCacheByUsername.Has(username))
			{
				_agentSessionCacheByUsername.Remove(username);
			}
		}

		public IApplicationClarifySession GetApplicationSession()
        {
			return getSession(_settings.ApplicationUsername);
        }

		private IApplicationClarifySession getSession(string username)
	    {
			var session = _agentSessionCacheByUsername[username];

		    if (session != null && ClarifyApplication.IsSessionValid(session.Id))
		    {
			    return session;
		    }

			_agentSessionCacheByUsername.Remove(username);
		    return getSession(username);
	    }

	    private IApplicationClarifySession wrapSession(ClarifySession session)
        {
            return new ClarifySessionWrapper(session);
        }
    }
}