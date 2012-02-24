using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Token;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests.Token
{
    [TestFixture]
    public class reset_token_api : Context<TokenAuthenticationApi>
    {
        const string username = "username";
        const string password = "password";
        
        [Test]
        public void reset_fails_when_authentication_fails()
        {
            MockFor<IUserAuthenticator>().Stub(s => s.Authenticate(username, password)).Return(false);

            var result = _cut.ResetToken(username, password);

            result.Token.ShouldBeNull();
        }

        [Test]
        public void reset_should_return_new_token_when_authentication_succeeds()
        {
            const string token = "newtoken";
            MockFor<IUserAuthenticator>().Stub(s => s.Authenticate(username, password)).Return(true);
            MockFor<IAuthenticationTokenRepository>().Stub(s => s.GenerateToken(username)).Return(new AuthenticationToken {Token = token});

            var result = _cut.ResetToken(username, password);

            result.Token.ShouldEqual(token);
        }
    }
}