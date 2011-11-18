using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.AuthToken
{
    public interface IUserAuthenticator
    {
        bool Authenticate(string username, string password);
    }

    public class UserAuthenticator : IUserAuthenticator
    {
        private readonly ILogger _logger;
        private readonly IClarifyApplicationFactory _clarifyApplicationFactory;

        public UserAuthenticator(ILogger logger, IClarifyApplicationFactory clarifyApplicationFactory)
        {
            _logger = logger;
            _clarifyApplicationFactory = clarifyApplicationFactory;
        }

        public bool Authenticate(string username, string password)
        {
            //HACK to make sure SDK is spun up. ICK
            _clarifyApplicationFactory.Create();

            var clarifyAuthenticationService = new ClarifyAuthenticationService();

            var result = clarifyAuthenticationService.AuthenticateUser(username, password);

            _logger.LogDebug("User {0} authentication was {1}successful.".ToFormat(username, result.Success ? "" : "not "));

            return result.Success;
        }
    }

}