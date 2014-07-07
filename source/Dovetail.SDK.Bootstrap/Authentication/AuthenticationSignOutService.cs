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
		private readonly IUserProxyService _proxyService;

		public AuthenticationSignOutService(ICurrentSDKUser currentSdkUser, 
			IFormsAuthenticationService formsAuthentication, 
			IUserProxyService proxyService)
		{
			_currentSdkUser = currentSdkUser;
			_formsAuthentication = formsAuthentication;
			_proxyService = proxyService;
		}

		public void SignOut()
		{
			_proxyService.CancelProxy(_currentSdkUser.ProxyUsername);

			_currentSdkUser.SignOut();
			_formsAuthentication.SignOut();
		}
	}
}