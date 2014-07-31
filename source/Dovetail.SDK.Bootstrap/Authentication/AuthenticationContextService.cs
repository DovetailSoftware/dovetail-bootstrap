using Dovetail.SDK.Bootstrap.Authentication.Principal;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IAuthenticationContextService
	{
		void SetupAuthenticationContext();
	}

	public class AuthenticationContextService : IAuthenticationContextService
	{
		private readonly ICurrentSDKUser _currentSdkUser;
		private readonly ILogger _logger;
		private readonly IPrincipalFactory _principalFactory;
		private readonly ISecurityContext _securityContext;

		public AuthenticationContextService(ISecurityContext securityContext, IPrincipalFactory principalFactory, ICurrentSDKUser currentSdkUser, ILogger logger)
		{
			_securityContext = securityContext;
			_principalFactory = principalFactory;
			_currentSdkUser = currentSdkUser;
			_logger = logger;
		}

		public void SetupAuthenticationContext()
		{
			var identity = _securityContext.CurrentIdentity;

			if (!identity.IsAuthenticated)
			{
				_logger.LogWarn("User '{0}' does not seem to be authenticated. Not setting up authentication.", identity.Name);
				return;
			}

			var principal = _principalFactory.CreatePrincipal(identity.Name);

			if (principal == null)
			{
				_logger.LogWarn("User '{0}'seems to have been authenticated but a principal could not be created. Not setting up authentication.", identity.Name);
				return;
			}

			_securityContext.CurrentUser = principal;
			_currentSdkUser.SetUser(principal.Identity.Name);
		}
	}
}