using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IAuthenticationService
	{
		bool SignIn(string username, string password, bool rememberMe);
	}

	public class AuthenticationService : IAuthenticationService
    {
        private readonly ICurrentSDKUser  _currentSdkUser;
        private readonly IFormsAuthenticationService _formsAuthentication;
        private readonly IUserAuthenticator _agentAuthenticator;
        private readonly IPrincipalFactory _principalFactory;
		private readonly ILogger _logger;

		public AuthenticationService(ICurrentSDKUser currentSdkUser, IFormsAuthenticationService formsAuthentication, IUserAuthenticator agentAuthenticator, IPrincipalFactory principalFactory, ILogger logger)
        {
            _currentSdkUser = currentSdkUser;
            _formsAuthentication = formsAuthentication;
            _agentAuthenticator = agentAuthenticator;
            _principalFactory = principalFactory;
			_logger = logger;
        }

        public bool SignIn(string username, string password, bool rememberMe)
        {
			_logger.LogDebug("Authenticating session {0}.".ToFormat(username));

            var authenticated = _agentAuthenticator.Authenticate(username, password);

            if (!authenticated)
            {
				return false;
            }
            
            var identity = new GenericIdentity(username);
            _currentSdkUser.SetUser(_principalFactory.CreatePrincipal(identity));

            _formsAuthentication.SetAuthCookie(username, rememberMe);

            return true;
        }
    }
}