using FChoice.Foundation.Clarify;
using FubuCore;
using FubuCore.Util;

namespace Dovetail.SDK.Bootstrap.Clarify
{
	public interface IClarifySessionCache
    {
        IClarifySession GetSession(string username);
		IApplicationClarifySession GetApplicationSession();
    }

    public class ClarifySessionCache : IClarifySessionCache
    {
	    private const string ApplicationSessionUserKey = "___ApplicationUser___";
	    private readonly IClarifyApplicationFactory _clarifyApplicationFactory;
        private readonly ILogger _logger;
    	private readonly IUserClarifySessionConfigurator _sessionConfigurator;

        private ClarifyApplication _clarifyApplication;
		private readonly Cache<string, IApplicationClarifySession> _agentSessionCacheByUsername = new Cache<string, IApplicationClarifySession>();
		
        public ClarifySessionCache(IClarifyApplicationFactory clarifyApplicationFactory, ILogger logger, IUserClarifySessionConfigurator sessionConfigurator)
        {
            _clarifyApplicationFactory = clarifyApplicationFactory;
            _logger = logger;
        	_sessionConfigurator = sessionConfigurator;
        	_agentSessionCacheByUsername.OnMissing = onAgentMissing;
        }

        public ClarifyApplication ClarifyApplication
        {
            get { return _clarifyApplication ?? (_clarifyApplication = _clarifyApplicationFactory.Create()); }
        }

		private IApplicationClarifySession onAgentMissing(string username)
        {
            _logger.LogDebug("Creating missing session for agent {0}.".ToFormat(username));

			var clarifySession = (username == ApplicationSessionUserKey) ? ClarifyApplication.CreateSession() : ClarifyApplication.CreateSession(username, ClarifyLoginType.User);

			_sessionConfigurator.Configure(clarifySession);

			_logger.LogDebug("Created and configured session {0} for agent {1}.".ToFormat(clarifySession.SessionID, username));

            return wrapSession(clarifySession);
        }

	    public IClarifySession GetSession(string username)
		{
			return getSession(username);
		}


	    public IApplicationClarifySession GetApplicationSession()
        {
	        return getSession(ApplicationSessionUserKey);
        }

	    private IApplicationClarifySession getSession(string username)
	    {
		    var session = _agentSessionCacheByUsername[username];

		    if (ClarifyApplication.IsSessionValid(session.Id))
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