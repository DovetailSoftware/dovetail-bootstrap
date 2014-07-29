using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public interface IContactAuthenticator
	{
		bool Authenticate(string username, string password);
	}

	public class ContactAuthenticator : IContactAuthenticator
	{
		private readonly ILogger _logger;
		//private readonly IClarifyApplication _clarifyApplication;

		public ContactAuthenticator(ILogger logger) //, IClarifyApplication clarifyApplication)
		{
			_logger = logger;

			////HACK to make sure SDK is spun up. ICK
			//_clarifyApplication = clarifyApplication;
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
		private readonly IContactAuthenticator _authenticator;

		public ContactAuthenticationService(IFormsAuthenticationService formsAuthentication, IContactAuthenticator authenticator)
		{
			_formsAuthentication = formsAuthentication;
			_authenticator = authenticator;
		}

		public bool SignIn(string username, string password, bool rememberMe)
		{
			var authenticated = _authenticator.Authenticate(username, password);

			if (!authenticated) return false;

			_formsAuthentication.SetAuthCookie(username, rememberMe);

			return true;
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

			var principal = _principalFactory.CreatePrincipal(identity.Name);

			_securityContext.CurrentUser = principal;
		}
	}
}