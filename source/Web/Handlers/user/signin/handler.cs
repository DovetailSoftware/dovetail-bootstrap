using Bootstrap.Web.Handlers.home;
using Dovetail.SDK.Fubu.Authentication;
using FubuCore;
using FubuMVC.Core.Continuations;
using FubuMVC.Core.Urls;

namespace Bootstrap.Web.Handlers.user.signin
{
    public class get_handler
    {
        public SignInModel Execute(SignInRequest request)
        {
            return new SignInModel {ReturnUrl = request.ReturnUrl};
        }
    }

    public class post_handler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUrlRegistry _urlRegistry;

        public post_handler(IAuthenticationService authenticationService, IUrlRegistry urlRegistry)
        {
            _authenticationService = authenticationService;
            _urlRegistry = urlRegistry;
        }

        public FubuContinuation Execute(SignInModel model)
        {
            var loggedin = _authenticationService.SignIn(model.UserName, model.Password, true);

            if (loggedin)
            {
                return FubuContinuation.RedirectTo(model.ReturnUrl.IsEmpty() ? _urlRegistry.UrlFor<HomeRequest>() : model.ReturnUrl);
            }
            
            return FubuContinuation.TransferTo(new SignInRequest { ReturnUrl = model.ReturnUrl, LoginFailed = true });
        }
    }

    public class SignInRequest
    {
        //[QueryString]
        public string ReturnUrl { get; set; }
        //[QueryString]
        public bool LoginFailed { get; set; }
    } 

    public class SignInModel
    {
        public SignInModel()
        {
            ReturnUrl = "";
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
        public bool LoginFailed { get; set; }
    }
}