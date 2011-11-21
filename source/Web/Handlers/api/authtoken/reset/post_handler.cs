using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Token;

namespace Bootstrap.Web.Handlers.api.authtoken.reset
{
    public class post_handler
    {
        private readonly ITokenAuthenticationApi _api;

        public post_handler(ITokenAuthenticationApi api)
        {
            _api = api;
        }

        public AuthenticationTokenReset Execute(ResetAuthTokenRequest request)
        {
            return AuthenticationTokenReset.Create(_api.ResetToken(request.Username, request.Password));
        }
    }

    public class AuthenticationTokenReset : AuthenticationToken
    {
        public static AuthenticationTokenReset Create(IAuthenticationToken token)
        {
            return new AuthenticationTokenReset
            {
                Username = token.Username,
                Token = token.Token,
            };
        }
    }
    
    public class ResetAuthTokenRequest : IUnauthenticatedApi
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}