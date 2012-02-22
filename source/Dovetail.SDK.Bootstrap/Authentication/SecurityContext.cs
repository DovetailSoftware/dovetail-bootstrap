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

    public class SecurityContext : ISecurityContext
    {
        private readonly HttpContextBase _context;

        public SecurityContext(HttpContextBase httpContext)
        {
            _context = httpContext;
        }


        public bool IsAuthenticated()
        {
            return _context.Request.IsAuthenticated;
        }

        public IIdentity CurrentIdentity { get { return _context.User.Identity; } }

        public IPrincipal CurrentUser { get { return _context.User; } set { _context.User = value; } }
    }
}