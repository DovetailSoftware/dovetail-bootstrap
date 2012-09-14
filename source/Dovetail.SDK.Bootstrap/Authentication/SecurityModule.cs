using System;
using System.Web;
using StructureMap;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public class SecurityModule : IHttpModule
    {
		private ISecurityContext _securityContext;

		public void Init(HttpApplication context)
        {
			_securityContext = ObjectFactory.Container.GetInstance<ISecurityContext>();
            context.AuthenticateRequest += onContextOnAuthenticateRequest;
        }

        private void onContextOnAuthenticateRequest(object sender, EventArgs e)
        {
	        if (RequiresAuthentication(_securityContext))
            {
                ObjectFactory.Container.GetInstance<IAuthenticationContextService>().SetupAuthenticationContext();
            }
        }

		public bool RequiresAuthentication(ISecurityContext securityContext)
		{
			return securityContext.RequiresAuthentication() && securityContext.IsAuthenticated();
		}
        
        public void Dispose()
        {
        }
    }
}