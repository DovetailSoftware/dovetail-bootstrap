using System.Collections.Generic;
using System.Net;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Token;
using FubuCore.Binding;
using FubuMVC.Core.Runtime;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests.Token
{
    public class authentication_token_behavior : Context<AuthenticationTokenBehavior>
    {
        private AggregateDictionary _aggregateDictionary;
        private string _token;
        private Dictionary<string, object> _requestDictionary;

        public override void Given()
        {
            _token = "token";
            _requestDictionary = new Dictionary<string, object> { { "authToken", _token } };

            _aggregateDictionary.AddDictionary("Other", _requestDictionary);

			MockFor<IFubuRequest>().Stub(a => a.Get<ICurrentSDKUser>()).Return(MockFor<ICurrentSDKUser>());
        }

        public override void OverrideMocks()
        {
            _aggregateDictionary = new AggregateDictionary();
            Override(_aggregateDictionary);
        }

        [Test] 
        public void token_should_be_found_on_request()
        {
            _cut.Invoke(); 
            
            MockFor<IAuthenticationTokenRepository>().AssertWasCalled(a => a.RetrieveByToken(_token));
        }
        
        [Test]
        public void should_401_when_authentiation_token_is_not_on_request()
        {
            _requestDictionary.Clear();

            _cut.Invoke();

            MockFor<IOutputWriter>().AssertWasCalled(a => a.WriteResponseCode(HttpStatusCode.Unauthorized));
        }

        [Test]
        public void should_401_when_no_authentiation_token_is_retrieved()
        {
            MockFor<IAuthenticationTokenRepository>().Stub(a => a.RetrieveByToken(_token)).Return(null);

            _cut.Invoke();

            MockFor<IOutputWriter>().AssertWasCalled(a => a.WriteResponseCode(HttpStatusCode.Unauthorized));
        }

        [Test]
        public void should_set_authentication_token_on_fubu_request_when_validated()
        {
            IAuthenticationToken authToken = new AuthenticationToken { Token = _token };
            MockFor<IAuthenticationTokenRepository>().Stub(a => a.RetrieveByToken(_token)).Return(authToken);

            _cut.Invoke();

            MockFor<IFubuRequest>().AssertWasCalled(a => a.Set(authToken));
        }

		[Test]
		public void should_set_current_sdk_user_when_validated()
		{
		    const string username = "annie";
		    IAuthenticationToken authToken = new AuthenticationToken { Token = _token, Username = username};
			MockFor<IAuthenticationTokenRepository>().Stub(a => a.RetrieveByToken(_token)).Return(authToken);

			_cut.Invoke();
			
			MockFor<ICurrentSDKUser>().AssertWasCalled(s=>s.SetUserName(username));
		}

    }
}