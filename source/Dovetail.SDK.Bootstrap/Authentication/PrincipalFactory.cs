using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	/// <summary>
	///     Responsibile for creation of the current user's principal.
	/// </summary>
	public interface IPrincipalFactory
	{
		IPrincipal CreatePrincipal(IIdentity identity);
		IPrincipal CreatePrincipal(string username);
	}

	public class PrincipalFactory : IPrincipalFactory
	{
		private readonly IFormsAuthenticationService _formsAuthenticationService;
		private readonly ILogger _logger;
		private readonly IClarifySessionCache _sessionCache;

		public PrincipalFactory(IClarifySessionCache sessionCache, ILogger logger, IFormsAuthenticationService formsAuthenticationService)
		{
			_sessionCache = sessionCache;
			_logger = logger;
			_formsAuthenticationService = formsAuthenticationService;
		}

		public IPrincipal CreatePrincipal(IIdentity identity)
		{
			var username = identity.Name;

			try
			{
				//if agent get session to validate it. Yay! Exceptions as control flow!
				_sessionCache.GetSession(username);
				_logger.LogDebug("Creating principal for user {0}.", username);

				return new GenericPrincipal(identity, new string[0]);
			}
			catch (FCInvalidLoginException e)
			{
				_logger.LogError("User was authenticated as but does not match a valid Clarify login. Attempting to sign out the user.", e);
				_formsAuthenticationService.SignOut();
				return null;
			}
		}

		public IPrincipal CreatePrincipal(string username)
		{
			return CreatePrincipal(new GenericIdentity(username));
		}
	}
}