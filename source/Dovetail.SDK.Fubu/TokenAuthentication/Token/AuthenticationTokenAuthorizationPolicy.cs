using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Token;
using FubuCore;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Security;
using IPrincipalFactory = Dovetail.SDK.Bootstrap.Authentication.IPrincipalFactory;

namespace Dovetail.SDK.Fubu.TokenAuthentication.Token
{
    public class AuthenticationTokenRequest
    {
        public string authToken { get; set; }
    }

    public class AuthenticationTokenAuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly ICurrentSDKUser _currentSdkUser;
        private readonly IAuthenticationTokenRepository _tokenRepository;
        private readonly IPrincipalFactory _principalFactory;
        private readonly ILogger _logger;

        public AuthenticationTokenAuthorizationPolicy(ICurrentSDKUser currentSdkUser, IAuthenticationTokenRepository tokenRepository, IPrincipalFactory principalFactory, ILogger logger)
        {
            _currentSdkUser = currentSdkUser;
            _tokenRepository = tokenRepository;
            _principalFactory = principalFactory;
            _logger = logger;
        }

        public AuthorizationRight RightsFor(IFubuRequest request)
        {
            var authToken = request.Get<AuthenticationTokenRequest>();

            //Workaround: RightsFor is getting called multiple times because of a Fubu bug 
            if(request.Has<IAuthenticationToken>()) return AuthorizationRight.Allow;

            var token = authToken.authToken;

            if(token.IsEmpty())
            {
                if(_currentSdkUser.IsAuthenticated)
                {
                    _logger.LogDebug("No AuthToken was found in this request but a user is authenticated. Using the current user's credentials.");
                    return AuthorizationRight.Allow;
                }
                
                return AuthorizationRight.Deny;
            }

            _logger.LogDebug("Authentication token {0} found.", token);

            var authenticationToken = _tokenRepository.RetrieveByToken(token);
            if (authenticationToken == null)
            {
                return AuthorizationRight.Deny;

            }

            _logger.LogDebug("Authentication token {0} found and validated for user {1}.", authenticationToken, authenticationToken);
            request.Set(authenticationToken);

            _currentSdkUser.SetUser(_principalFactory.CreatePrincipal(authenticationToken.Username));

            return AuthorizationRight.Allow;
        }
    }
}