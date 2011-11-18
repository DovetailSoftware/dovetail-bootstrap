using Dovetail.SDK.Bootstrap.AuthToken;

namespace Bootstrap.Web.Handlers.api.authtoken.retrieve
{
    public class post_handler
    {
        private readonly ITokenAuthenticationApi _api;

        public post_handler(ITokenAuthenticationApi api)
        {
            _api = api;
        }

        public TokenAuthenticationResult Execute(RetrieveAuthTokenRequest request)
        {
            return _api.GetToken(request.Username, request.Password);
        }
    }

    public class RetrieveAuthTokenRequest 
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}