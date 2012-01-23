using Dovetail.SDK.Fubu.Authentication;
using FubuCore;
using FubuMVC.Core.Continuations;

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

        public post_handler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public FubuContinuation Execute(SignInModel model)
        {
            var loggedin = _authenticationService.SignIn(model.UserName, model.Password, true);

            return loggedin ? FubuContinuation.RedirectTo(model.ReturnUrl.IsEmpty() ? "/" : model.ReturnUrl)
                       : FubuContinuation.TransferTo(new SignInRequest { ReturnUrl = model.ReturnUrl, LoginFailed = true });
        }
    }

    public class SignInRequest
    {
        public string ReturnUrl { get; set; }
        public bool LoginFailed { get; set; }
    } 

    public class SignInModel
    {
        public SignInModel()
        {
            ReturnUrl = "/";
        }

        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }
        public bool LoginFailed { get; set; }
    }
}