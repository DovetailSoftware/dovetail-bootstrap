using System;
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
        private readonly IClarifyApplicationFactory _clarifyApplicationFactory;
        private readonly ILogger _logger;
    	private readonly IUserClarifySessionConfigurator _sessionConfigurator;

        private ClarifyApplication _clarifyApplication;
        private readonly Cache<string, Guid> _agentSessionCacheByUsername = new Cache<string, Guid>();
        private Guid _applicationSessionId;

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

        private Guid onAgentMissing(string username)
        {
            _logger.LogDebug("Creating missing session for agent {0}.".ToFormat(username));

            var clarifySession = ClarifyApplication.CreateSession(username, ClarifyLoginType.User);

            _logger.LogDebug("Created session {0} for agent {1}.".ToFormat(clarifySession.SessionID, username));

            return clarifySession.SessionID;
        }

        public IClarifySession GetSession(string username)
        {
            var sessionId = _agentSessionCacheByUsername[username];

            try
            {
                var session = ClarifyApplication.GetSession(sessionId);
            	_sessionConfigurator.Configure(session);
             
                return wrapSession(session);
            }
            catch (Exception exception)
            {
                _agentSessionCacheByUsername.Remove(username);
				if (exception.GetType().CanBeCastTo<ClarifyException>() && ((ClarifyException)exception).ErrorCode == 15056)
				{
					_logger.LogDebug("Could not retrieve agent session for {0} via id {1} because it expired. Creating a new one. Error: {2}".ToFormat(username, sessionId, exception.Message));
					return GetSession(username);
				}
				_logger.LogError("Getting the session for {0} with id {1} failed but not because it was expired.".ToFormat(username, sessionId), exception);
				throw new ApplicationException("Could not create a session for {0} with id {1}.".ToFormat(username,sessionId), exception);
            }
        }

        public IApplicationClarifySession GetApplicationSession()
        {
            try
            {
                if (_applicationSessionId == Guid.Empty)
                {
                    var session = ClarifyApplication.CreateSession();
                    _applicationSessionId = session.SessionID;
                    return wrapSession(session);
                }
                
                return wrapSession(ClarifyApplication.GetSession(_applicationSessionId));
            }
            catch (Exception exception)
            {
				_applicationSessionId = Guid.Empty;
				if (exception.GetType().CanBeCastTo<ClarifyException>() && ((ClarifyException)exception).ErrorCode == 15056)
				{
					_logger.LogDebug("Could not retrieve application session via id {0} because it expired. Creating a new one. Error: {1}".ToFormat(_applicationSessionId, exception.Message));
					return GetApplicationSession();
				}

				throw new ApplicationException("Could not create an application session with id {0}.".ToFormat(_applicationSessionId), exception);
            }
        }

		private IApplicationClarifySession wrapSession(ClarifySession session)
        {
            return new ClarifySessionWrapper(session);
        }
    }
}