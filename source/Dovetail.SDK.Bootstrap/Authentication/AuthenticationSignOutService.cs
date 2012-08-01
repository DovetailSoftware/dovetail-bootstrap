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

		public AuthenticationSignOutService(ICurrentSDKUser currentSdkUser, IFormsAuthenticationService formsAuthentication, IClarifySessionCache sessionCache)
		{
			_currentSdkUser = currentSdkUser;
			_formsAuthentication = formsAuthentication;
			_sessionCache = sessionCache;
		}

		public void SignOut()
		{
			var username = _currentSdkUser.Username;
			_currentSdkUser.SignOut();
			_sessionCache.EjectSession(username);
			_formsAuthentication.SignOut();
		}
	}
}