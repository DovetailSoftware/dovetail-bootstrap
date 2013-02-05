using System.Net;
using Bootstrap.Web.Handlers.user.signin;
using FubuCore.Binding;
using FubuMVC.Core;
using FubuMVC.Core.Http;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Security;
using FubuMVC.Core.Urls;

namespace Bootstrap.Web.Security
{
    public class BootstrapAuthorizationFailureHandler : IAuthorizationFailureHandler
    {
        private readonly IOutputWriter _writer;
        private readonly IRequestData _requestData;
        private readonly IUrlRegistry _urlRegistry;
        private readonly ICurrentHttpRequest _currentHttpRequest;

        public BootstrapAuthorizationFailureHandler(IOutputWriter writer, IRequestData requestData, IUrlRegistry urlRegistry, ICurrentHttpRequest currentHttpRequest)
        {
            _writer = writer;
            _requestData = requestData;
            _urlRegistry = urlRegistry;
            _currentHttpRequest = currentHttpRequest;
        }

        public void Handle()
        {
            if (_requestData.IsAjaxRequest())
            {
                _writer.WriteResponseCode(HttpStatusCode.Forbidden);
                return;
            }
            
            //ICK - working around Fubu Url building bug 
            //var loginUrl = _urlRegistry.UrlFor(new SignInRequest {ReturnUrl = _currentHttpRequest.RawUrl()}); //this is what it should be

            //redirect to login url 
            var loginUrl = _urlRegistry.UrlFor(new SignInRequest());
            loginUrl = loginUrl.TrimEnd('/') + "?ReturnUrl=" + _currentHttpRequest.RawUrl().UrlEncode();

            _writer.RedirectToUrl(loginUrl);
        }
    }
}