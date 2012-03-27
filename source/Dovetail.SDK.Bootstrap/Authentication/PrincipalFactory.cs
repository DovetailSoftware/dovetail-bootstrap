using System.Linq;
using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    /// <summary>
    /// Responsibile for creation of the current users principal. Replace the default on e
    /// </summary>
    public interface IPrincipalFactory
    {
        IPrincipal CreatePrincipal(IIdentity identity);
        IPrincipal CreatePrincipal(string username);
    }

    public class PrincipalFactory : IPrincipalFactory
    {
        private readonly IClarifySessionCache _sessionCache;

        public PrincipalFactory(IClarifySessionCache sessionCache)
        {
            _sessionCache = sessionCache;
        }

        public IPrincipal CreatePrincipal(IIdentity identity)
        {
            var session = _sessionCache.GetSession(identity.Name);
            
            return new DovetailPrincipal(identity, session.Permissions);
        }

        public IPrincipal CreatePrincipal(string username)
        {
            return CreatePrincipal(new GenericIdentity(username));
        }
    }

    public class DovetailPrincipal : IPrincipal
    {
        private readonly IIdentity _identity;
        private readonly string[] _permissions;

        public DovetailPrincipal(IIdentity identity, string[] permissions)
        {
            _identity = identity;
            _permissions = permissions;
        }

        public bool IsInRole(string role)
        {
            return _permissions.Any(p => p == role);
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }
    }
}