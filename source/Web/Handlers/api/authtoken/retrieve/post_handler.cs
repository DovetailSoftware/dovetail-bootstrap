using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Token;

namespace Bootstrap.Web.Handlers.api.authtoken.retrieve
{
    public class post_handler
    {
        private readonly ITokenAuthenticationApi _api;

        public post_handler(ITokenAuthenticationApi api)
        {
            _api = api;
        }

        public AuthenticationToken Execute(RetrieveAuthTokenRequest request)
        {
            return AuthenticationTokenRequest.Create(_api.GetToken(request.Username, request.Password));
        }
    }

    public class AuthenticationTokenRequest : AuthenticationToken
    {
        public static AuthenticationTokenRequest Create(IAuthenticationToken token)
        {
            return new AuthenticationTokenRequest
            {
                Username = token.Username,
                Token = token.Token,
            };
        }
    }


    public class RetrieveAuthTokenRequest : IUnauthenticatedApi
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}