using System.Web.Security;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface IFormsAuthenticationService
    {
        void SetAuthCookie(string username, bool createPersistentCookie);
        void SignOut();
    }
    
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SetAuthCookie(string username, bool createPersistentCookie)
        {
            FormsAuthentication.SetAuthCookie(username, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
}