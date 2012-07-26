using Bootstrap.Web.Handlers.home;
using Dovetail.SDK.Bootstrap.Authentication;
using FubuMVC.Core.Continuations;

namespace Bootstrap.Web.Handlers.user.signout
{
    public class get_handler
    {
		private readonly IAuthenticationSignOutService _signOutService;

        public get_handler(IAuthenticationSignOutService signOutService)
        {
            _signOutService = signOutService;
        }

        public FubuContinuation Execute(SignOutRequest request)
        {
            _signOutService.SignOut();

            return FubuContinuation.RedirectTo<HomeRequest>();
        }
    }

    public class SignOutRequest { }
}