using System;
using System.Web;
using StructureMap;

namespace Dovetail.SDK.Bootstrap.Authentication
{
	public class SecurityModule : IHttpModule
    {
       public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += onContextOnAuthenticateRequest;
        }

        private void onContextOnAuthenticateRequest(object sender, EventArgs e)
        {
	        var securityContext = ObjectFactory.Container.GetInstance<ISecurityContext>();
	        if (RequiresAuthentication(securityContext))
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