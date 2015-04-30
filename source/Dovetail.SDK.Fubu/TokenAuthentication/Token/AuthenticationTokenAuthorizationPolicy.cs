using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Token;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Security;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
	public class AuthenticationTokenRequest
	{
		public string authToken { get; set; }
	}

	public class AuthenticationTokenAuthorizationPolicy : IAuthorizationPolicy
	{
		public AuthorizationRight RightsFor(IFubuRequestContext request)
		{
			var currentSdkUser = request.Service<ICurrentSDKUser>();
			var tokenRepository = request.Service<IAuthenticationTokenRepository>();
			var logger = request.Service<ILogger>();
			var authToken = request.Models.Get<AuthenticationTokenRequest>();

			//Workaround: RightsFor is getting called multiple times because of a Fubu bug 
			if (request.Models.Has<IAuthenticationToken>()) return AuthorizationRight.Allow;

			var token = authToken.authToken;

			if (token.IsEmpty())
			{
				if (currentSdkUser.IsAuthenticated)
				{
					logger.LogDebug("No AuthToken was found in this request but a user is already authenticated. Using the current user's credentials.");
					return AuthorizationRight.Allow;
				}

				return AuthorizationRight.Deny;
			}

			logger.LogDebug("Authentication token {0} found.", token);

			var authenticationToken = tokenRepository.RetrieveByToken(token);
			if (authenticationToken == null)
			{
				return AuthorizationRight.Deny;

			}

			logger.LogDebug("Authentication token {0} found and validated for user {1}.", authenticationToken, authenticationToken);
			request.Models.Set(authenticationToken);

			currentSdkUser.SetUser(authenticationToken.Username);

			return AuthorizationRight.Allow;
		}
	}
}