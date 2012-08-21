using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public class ContactAuthenticator : IUserAuthenticator
    {
        private readonly ILogger _logger;
        private readonly IClarifyApplication _clarifyApplication;

        public ContactAuthenticator(ILogger logger, IClarifyApplication clarifyApplication)
        {
            _logger = logger;
			
			//HACK to make sure SDK is spun up. ICK
			_clarifyApplication = clarifyApplication;
        }

        public bool Authenticate(string username, string password)
        {
            var success = ClarifySession.AuthenticateContact(username, password);

            _logger.LogDebug("Authentication for contact {0} was {1}successful.".ToFormat(username, success ? "" : "not "));

            return success;
        }
    }

    public class ContactAuthenticationService : IAuthenticationService
    {
        private readonly IFormsAuthenticationService _formsAuthentication;
        private readonly IUserAuthenticator _agentAuthenticator;
	    private readonly IAuthenticationSignOutService _signOutService;

	    public ContactAuthenticationService(IFormsAuthenticationService formsAuthentication, ContactAuthenticator agentAuthenticator, IAuthenticationSignOutService signOutService)
        {
            _formsAuthentication = formsAuthentication;
            _agentAuthenticator = agentAuthenticator;
	        _signOutService = signOutService;
        }

        public bool SignIn(string username, string password, bool rememberMe)
        {
            var authenticated = _agentAuthenticator.Authenticate(username, password);

            if (!authenticated) return false;

            _formsAuthentication.SetAuthCookie(username, rememberMe);

            return true;
        }

        public void SignOut()
        {
            _signOutService.SignOut();
        }
    }

    public class ContactAuthenticationContextService : IAuthenticationContextService
    {
        private readonly ISecurityContext _securityContext;
        private readonly IPrincipalFactory _principalFactory;

        public ContactAuthenticationContextService(ISecurityContext securityContext, IPrincipalFactory principalFactory)
        {
            _securityContext = securityContext;
            _principalFactory = principalFactory;
        }

        public void SetupAuthenticationContext()
        {
            var identity = _securityContext.CurrentIdentity;

            var principal = _principalFactory.CreatePrincipal(identity);

            _securityContext.CurrentUser = principal;
        }
    }
}