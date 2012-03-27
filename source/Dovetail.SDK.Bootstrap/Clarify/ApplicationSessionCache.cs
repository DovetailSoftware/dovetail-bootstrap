using System;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public interface IApplicationSessionCache
    {
        IApplicationClarifySession GetApplicationSession();
    }
    
    public class ApplicationSessionCache : IApplicationSessionCache
    {
        private readonly IClarifyApplicationFactory _clarifyApplicationFactory;
        private readonly ILogger _logger;
        //TODO configure StructureMap to do the Create() for us
        private ClarifyApplication _clarifyApplication;
        private Guid _applicationSessionId;

        public ApplicationSessionCache(IClarifyApplicationFactory clarifyApplicationFactory, ILogger logger)
        {
            _clarifyApplicationFactory = clarifyApplicationFactory;
            _logger = logger;
        }

        public ClarifyApplication ClarifyApplication
        {
            get { return _clarifyApplication ?? (_clarifyApplication = _clarifyApplicationFactory.Create()); }
        }
        
        //todo integration tests!!!!!!
        public IApplicationClarifySession GetApplicationSession()
        {
            try
            {
                if (_applicationSessionId == Guid.Empty)
                {
                    return CreateSession();
                }

                return wrapSession(ClarifyApplication.GetSession(_applicationSessionId));
            }
            catch(ClarifyException clarifyException)
            {
                if (clarifyException.ErrorCode != 15056)
                {
                    throw;
                }
                
                _logger.LogDebug("Could not retrieve application session via id {0}. Likely it expired. Creating a new one. Error: {1}".ToFormat(_applicationSessionId, clarifyException.Message));
                return CreateSession();
            }
            catch(Exception ex)
            {
                _logger.LogError("Application session could not be created.", ex);
                throw;
            }
        }

        private IApplicationClarifySession CreateSession()
        {
            var session = ClarifyApplication.CreateSession();
            _applicationSessionId = session.SessionID;
            return wrapSession(session);
        }

        private static IApplicationClarifySession wrapSession(ClarifySession session)
        {
            return new ClarifySessionWrapper(session);
        }
    }
}