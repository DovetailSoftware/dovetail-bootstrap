using System.Web.Configuration;

namespace Dovetail.SDK.Bootstrap.Authentication.Principal
{
	public interface IPrincipalValidatorFactory
	{
		IPrincipalValidator Create();
	}

	public class PrincipalValidatorFactory : IPrincipalValidatorFactory
	{
		private readonly WindowsAuthenticationPrincipalValidator _windowsAuthValidator;
		private readonly FormsAuthenticationPrincipalValidator _formsAuthValidator;
		private readonly NoAuthPrincipalValidator _noAuthValidator;

		public PrincipalValidatorFactory(WindowsAuthenticationPrincipalValidator windowsAuthValidator,
			FormsAuthenticationPrincipalValidator formsAuthValidator,
			NoAuthPrincipalValidator noAuthValidator
			)
		{
			_windowsAuthValidator = windowsAuthValidator;
			_formsAuthValidator = formsAuthValidator;
			_noAuthValidator = noAuthValidator;
		}

		public IPrincipalValidator Create()
		{
			var configuration = WebConfigurationManager.OpenWebConfiguration("~");
			var authenticationSection = (AuthenticationSection)configuration.GetSection("system.web/authentication");
			if (authenticationSection.Mode == AuthenticationMode.Windows)
			{
				return _windowsAuthValidator;
			}

			if (authenticationSection.Mode == AuthenticationMode.Forms)
			{
				return _formsAuthValidator;
			}

			return _noAuthValidator;
		}
	}
}