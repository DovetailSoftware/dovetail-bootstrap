using System;

namespace Dovetail.SDK.Bootstrap.Authentication.Principal
{
	public class FormsAuthenticationPrincipalValidator : IPrincipalValidator
	{
		private readonly IFormsAuthenticationService _formsAuthenticationService;
		private readonly ILogger _logger;

		public FormsAuthenticationPrincipalValidator(IFormsAuthenticationService formsAuthenticationService,
			ILogger logger)
		{
			_formsAuthenticationService = formsAuthenticationService;
			_logger = logger;
		}

		public void FailureHandler(Exception ex)
		{
			_logger.LogError("User was authenticated as but does not match a valid Clarify login. Attempting to sign out the user.", ex);
			_formsAuthenticationService.SignOut();
		}

		public string UserValidator(string username)
		{
			//The user validation is really the creation fo the session. If that fails the failure handle will be invoked.
			return username;
		}
	}
}