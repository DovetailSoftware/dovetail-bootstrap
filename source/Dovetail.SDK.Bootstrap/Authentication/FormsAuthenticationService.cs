using System.Web.Security;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface IFormsAuthenticationService
    {
        void SetAuthCookie(string username, bool createPersistentCookie);
        void SignOut();
    }
    
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        private readonly ILogger _logger;

        public FormsAuthenticationService(ILogger logger)
        {
            _logger = logger;
        }

        public void SetAuthCookie(string username, bool createPersistentCookie)
        {
            _logger.LogDebug("Setting {0}persistent authorization cookie for {1}.".ToFormat(createPersistentCookie ? "" : "non-", username));
            FormsAuthentication.SetAuthCookie(username, createPersistentCookie);
        }

        public void SignOut()
        {
            _logger.LogDebug("Signing out user.");
            FormsAuthentication.SignOut();
        }
    }
}