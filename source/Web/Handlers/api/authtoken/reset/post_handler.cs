using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.AuthToken;

namespace Bootstrap.Web.Handlers.api.authtoken.reset
{
    public class post_handler
    {
        private readonly ITokenAuthenticationApi _api;

        public post_handler(ITokenAuthenticationApi api)
        {
            _api = api;
        }

        public TokenAuthenticationResetResult Execute(ResetAuthTokenRequest request)
        {
            return _api.ResetToken(request.Username, request.Password);
        }
    }

    public class ResetAuthTokenRequest : IApi
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}