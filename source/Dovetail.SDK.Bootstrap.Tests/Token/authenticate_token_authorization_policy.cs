using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Token;
using Dovetail.SDK.Fubu.TokenAuthentication.Token;
using FubuMVC.Core;
using FubuMVC.Core.Http;
using FubuMVC.Core.Runtime;
using FubuMVC.Core.Security;
using FubuMVC.StructureMap;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests.Token
{
    public class authenticate_token_authorization_policy : Context<AuthenticationTokenAuthorizationPolicy>
    {
        private string _token;
        private IFubuRequest _request;
	    private IFubuRequestContext _context;
        private AuthenticationTokenRequest _authenticationTokenRequest;

        public override void Given()
        {
            _token = "token";
            _authenticationTokenRequest = new AuthenticationTokenRequest {authToken = _token};
            
            _request = MockFor<IFubuRequest>(); 
            _request.Stub(a => a.Get<AuthenticationTokenRequest>()).Return(_authenticationTokenRequest);
            
            _request.Stub(a => a.Get<ICurrentSDKUser>()).Return(MockFor<ICurrentSDKUser>());

	        var services = new StructureMapServiceLocator(_services.Container);
	        _context = new FubuRequestContext(services, MockFor<IHttpRequest>(), _request, MockFor<IOutputWriter>(), MockFor<FubuCore.Logging.ILogger>());
        }

        [Test]
        public void token_should_be_found_on_request()
        {

            _cut.RightsFor(_context);

            MockFor<IAuthenticationTokenRepository>().AssertWasCalled(a => a.RetrieveByToken(_token));
        }

        [Test]
        public void should_allow_when_no_authentication_token_is_on_request_but_user_is_authenticated()
        {
            _authenticationTokenRequest.authToken = null;
            MockFor<ICurrentSDKUser>().Stub(a => a.IsAuthenticated).Return(true);

			var result = _cut.RightsFor(_context);

            result.ShouldEqual(AuthorizationRight.Allow);
        }

        [Test]
        public void should_deny_when_authentication_token_is_not_on_request()
        {
            _authenticationTokenRequest.authToken = null;

			var result = _cut.RightsFor(_context);

            result.ShouldEqual(AuthorizationRight.Deny);
        }

        [Test]
        public void should_deny_when_no_authentication_token_can_be_retrieved()
        {
            MockFor<IAuthenticationTokenRepository>().Stub(a => a.RetrieveByToken(_token)).Return(null);

			var result = _cut.RightsFor(_context);

            result.ShouldEqual(AuthorizationRight.Deny);
        }

        [Test]
        public void should_set_authentication_token_on_fubu_request_when_validated()
        {
            IAuthenticationToken authToken = new AuthenticationToken { Token = _token };
            MockFor<IAuthenticationTokenRepository>().Stub(a => a.RetrieveByToken(_token)).Return(authToken);

			_cut.RightsFor(_context);

            _request.AssertWasCalled(a => a.Set(authToken));
        }

        [Test]
        public void should_set_current_sdk_user_when_validated()
        {
            const string username = "annie";
            IAuthenticationToken authToken = new AuthenticationToken { Token = _token, Username = username };
            MockFor<IAuthenticationTokenRepository>().Stub(a => a.RetrieveByToken(_token)).Return(authToken);

			_cut.RightsFor(_context);

            MockFor<ICurrentSDKUser>().AssertWasCalled(s => s.SetUser(username));
        }
    }
}