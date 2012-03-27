using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface IAuthenticationService
    {
        bool SignIn(string username, string password, bool rememberMe);
        void SignOut();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICurrentSDKUser  _currentSdkUser;
        private readonly IFormsAuthenticationService _formsAuthentication;
        private readonly IUserAuthenticator _agentAuthenticator;
        private readonly IClarifySessionCache _sessionCache;
        private readonly IPrincipalFactory _principalFactory;

        public AuthenticationService(ICurrentSDKUser currentSdkUser, IFormsAuthenticationService formsAuthentication, IUserAuthenticator agentAuthenticator, IClarifySessionCache sessionCache, IPrincipalFactory principalFactory)
        {
            _currentSdkUser = currentSdkUser;
            _formsAuthentication = formsAuthentication;
            _agentAuthenticator = agentAuthenticator;
            _sessionCache = sessionCache;
            _principalFactory = principalFactory;
        }

        public bool SignIn(string username, string password, bool rememberMe)
        {
            var authenticated = _agentAuthenticator.Authenticate(username, password);

            if (!authenticated) return false;
            
            var identity = new GenericIdentity(username);
            _currentSdkUser.SetUser(_principalFactory.CreatePrincipal(identity));

            _formsAuthentication.SetAuthCookie(username, rememberMe);

            return true;
        }

        public void SignOut()
        {
            _currentSdkUser.SignOut();
            _sessionCache.CloseUserSession();
            _formsAuthentication.SignOut();
        }
    }
}