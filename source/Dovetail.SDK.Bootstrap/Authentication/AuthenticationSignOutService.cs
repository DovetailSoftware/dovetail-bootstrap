using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IAuthenticationSignOutService
	{
		void SignOut();
	}

	public class AuthenticationSignOutService : IAuthenticationSignOutService
	{
		private readonly ICurrentSDKUser _currentSdkUser;
		private readonly IFormsAuthenticationService _formsAuthentication;
		private readonly IClarifySessionCache _sessionCache;
		private readonly IUserImpersonationService _impersonationService;

		public AuthenticationSignOutService(ICurrentSDKUser currentSdkUser,
			IFormsAuthenticationService formsAuthentication,
			IClarifySessionCache sessionCache,
			IUserImpersonationService impersonationService)
		{
			_currentSdkUser = currentSdkUser;
			_formsAuthentication = formsAuthentication;
			_sessionCache = sessionCache;
			_impersonationService = impersonationService;
		}

		public void SignOut()
		{
			var user = _currentSdkUser.Username;
			_impersonationService.StopImpersonating(user);
			_sessionCache.EjectSession(user.ToLowerInvariant());

			_currentSdkUser.SignOut();
			_formsAuthentication.SignOut();
		}
	}
}