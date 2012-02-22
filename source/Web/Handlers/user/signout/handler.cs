using Bootstrap.Web.Handlers.home;
using Dovetail.SDK.Bootstrap.Authentication;
using FubuMVC.Core.Continuations;

namespace Bootstrap.Web.Handlers.user.signout
{
    public class get_handler
    {
        private readonly IAuthenticationService _authenticationService;

        public get_handler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public FubuContinuation Execute(SignOutRequest request)
        {
            _authenticationService.SignOut();

            return FubuContinuation.RedirectTo<HomeRequest>();
        }
    }

    public class SignOutRequest { }
}