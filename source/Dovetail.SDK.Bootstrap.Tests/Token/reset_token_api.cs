using Dovetail.SDK.Bootstrap.AuthToken;
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

            result.Success.ShouldBeFalse();
        }

        [Test]
        public void reset_succeeds_when_authentication_succeeds()
        {
            MockFor<IUserAuthenticator>().Stub(s => s.Authenticate(username, password)).Return(true);

            var result = _cut.ResetToken(username, password);

            result.Success.ShouldBeTrue();
        }

        [Test]
        public void reset_should_return_new_token_when_authentication_succeeds()
        {
            const string token = "newtoken";
            MockFor<IUserAuthenticator>().Stub(s => s.Authenticate(username, password)).Return(true);
            MockFor<IAuthTokenRepository>().Stub(s => s.Generate(username)).Return(token);

            var result = _cut.ResetToken(username, password);

            result.Token.ShouldEqual(token);
        }
    }
}