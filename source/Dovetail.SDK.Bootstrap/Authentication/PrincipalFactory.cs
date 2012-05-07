using System.Linq;
using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    /// <summary>
    /// Responsibile for creation of the current user's principal. 
    /// </summary>
    public interface IPrincipalFactory
    {
        IPrincipal CreatePrincipal(IIdentity identity);
        IPrincipal CreatePrincipal(string username);
    }

    public class PrincipalFactory : IPrincipalFactory
    {
        private readonly IClarifySessionCache _sessionCache;
        private readonly ILogger _logger;

        public PrincipalFactory(IClarifySessionCache sessionCache, ILogger logger)
        {
            _sessionCache = sessionCache;
            _logger = logger;
        }

        public IPrincipal CreatePrincipal(IIdentity identity)
        {
            var username = identity.Name;

            //if agent get session and use its permissions
            var session = _sessionCache.GetSession(username);
            _logger.LogDebug("Creating principal for user {0} with {1} permissions.", username, session.Permissions.Length);

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