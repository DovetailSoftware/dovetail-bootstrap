using System;
using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication.Principal
{
	/// <summary>
	///     Responsibile for creation of the current user's principal.
	/// </summary>
	public interface IPrincipalFactory
	{
		IPrincipal CreatePrincipal(string username);
	}

	public class PrincipalFactory : IPrincipalFactory
	{
		private readonly ILogger _logger;
		private readonly IPrincipalValidatorFactory _principalValidatorFactory;
		private readonly IClarifySessionCache _sessionCache;

		public PrincipalFactory(IClarifySessionCache sessionCache, 
			ILogger logger, 
			IPrincipalValidatorFactory principalValidatorFactory)
		{
			_sessionCache = sessionCache;
			_logger = logger;
			_principalValidatorFactory = principalValidatorFactory;
		}

		public IPrincipal CreatePrincipal(string username)
		{
			var validator = _principalValidatorFactory.Create();
				
			try
			{
				var validatedUsername = validator.UserValidator(username);

				var session = _sessionCache.GetSession(validatedUsername);
				_logger.LogDebug("Creating principal for user {0} with {1} roles sourced from permissions.", validatedUsername, session.Permissions.Length);

				return new GenericPrincipal(new GenericIdentity(validatedUsername, "bootstrap"), session.Permissions);
			}
			catch (Exception e)
			{
				validator.FailureHandler(e);
				return null;
			}
		}
	}
}