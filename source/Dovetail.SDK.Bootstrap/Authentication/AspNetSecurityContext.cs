using System.Security.Principal;
using System.Web;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface ISecurityContext
    {
        IIdentity CurrentIdentity { get; }
        IPrincipal CurrentUser { get; set; }
        bool IsAuthenticated();
    }

    public class NullSecurityContext : ISecurityContext
    {
        public IIdentity CurrentIdentity
        {
            get { return CurrentUser == null ? null : CurrentUser.Identity; }
        }
        
        public IPrincipal CurrentUser { get; set; }

        public bool IsAuthenticated()
        {
            return CurrentUser != null;
        }
    }

    public class AspNetSecurityContext : ISecurityContext
    {
        public bool IsAuthenticated()
        {
            return HttpContext.Current.Request.IsAuthenticated;
        }

        public IIdentity CurrentIdentity { get { return HttpContext.Current.User.Identity; } }

        public IPrincipal CurrentUser { get { return HttpContext.Current.User; } set { HttpContext.Current.User = value; } }
    }
}