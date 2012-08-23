using System.Security.Principal;
using System.Web;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface ISecurityContext
    {
        IIdentity CurrentIdentity { get; }
        IPrincipal CurrentUser { get; set; }
        bool IsAuthenticated();
	    bool RequiresAuthentication();
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

	    public bool RequiresAuthentication()
	    {
		    return true;
	    }
    }

	public class AspNetSecurityContext : ISecurityContext
    {
		private readonly IRequestPathAuthenticationPolicy _requestPathAuthenticationPolicy;

		public AspNetSecurityContext(IRequestPathAuthenticationPolicy requestPathAuthenticationPolicy)
		{
			_requestPathAuthenticationPolicy = requestPathAuthenticationPolicy;
		}

		public bool IsAuthenticated()
        {
            return HttpContext.Current.Request.IsAuthenticated;
        }

		public bool RequiresAuthentication()
		{
			return _requestPathAuthenticationPolicy.PathRequiresAuthentication(HttpContext.Current.Request.Path);
		}

        public IIdentity CurrentIdentity { get { return HttpContext.Current.User.Identity; } }

        public IPrincipal CurrentUser { get { return HttpContext.Current.User; } set { HttpContext.Current.User = value; } }
    }
}