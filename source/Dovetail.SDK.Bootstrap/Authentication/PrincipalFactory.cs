using System.Security.Principal;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface IPrincipalFactory
    {
        IPrincipal CreatePrincipal(IIdentity identity);
    }

    public class PrincipalFactory : IPrincipalFactory
    {
        public IPrincipal CreatePrincipal(IIdentity identity)
        {
            return new DovetailAgentPrincipal(identity);
        }
    }

    public class DovetailAgentPrincipal : IPrincipal
    {
        private readonly IIdentity _identity;
        
        public DovetailAgentPrincipal(IIdentity identity)
        {
            _identity = identity;
        }

        public bool IsInRole(string role)
        {
            //TODO get permissions working 
            //return ClarifySession.Permissions.Contains(role);

            return true;
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }
    }
}