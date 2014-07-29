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
		private readonly ICurrentSDKUser _currentSdkUser;
		private readonly IFormsAuthenticationService _formsAuthentication;
		private readonly IUserAuthenticator _agentAuthenticator;
		private readonly IPrincipalFactory _principalFactory;
		private readonly IUserImpersonationService _impersonationService;
		private readonly ILogger _logger;

		public AuthenticationService(ICurrentSDKUser currentSdkUser,
			IFormsAuthenticationService formsAuthentication,
			IUserAuthenticator agentAuthenticator,
			IPrincipalFactory principalFactory,
			IUserImpersonationService impersonationService,
			ILogger logger)
		{
			_currentSdkUser = currentSdkUser;
			_formsAuthentication = formsAuthentication;
			_agentAuthenticator = agentAuthenticator;
			_principalFactory = principalFactory;
			_impersonationService = impersonationService;
			_logger = logger;
		}

		public bool SignIn(string username, string password, bool rememberMe)
		{
			_logger.LogDebug("Authenticating session {0}.".ToFormat(username));

			var isAuthenticated = _agentAuthenticator.Authenticate(username, password);

			if (!isAuthenticated)
			{
				return false;
			}

			_impersonationService.StopImpersonating(username);

			_currentSdkUser.SetUser(_principalFactory.CreatePrincipal(username));

			_formsAuthentication.SetAuthCookie(username, rememberMe);

			return true;
		}
	}
}