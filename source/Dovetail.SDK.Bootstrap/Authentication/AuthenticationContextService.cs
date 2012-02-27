using Dovetail.SDK.Bootstrap.Clarify;

namespace Dovetail.SDK.Bootstrap.Authentication
{
    public interface IAuthenticationContextService
    {
        void SetupAuthenticationContext();
    }

    public class AuthenticationContextService : IAuthenticationContextService
    {
        private readonly ISecurityContext _securityContext;
        private readonly IPrincipalFactory _principalFactory;
        private readonly ICurrentSDKUser _currentUserContext;

        public AuthenticationContextService(ISecurityContext securityContext, IPrincipalFactory principalFactory, ICurrentSDKUser currentUserContext)
        {
            _securityContext = securityContext;
            _principalFactory = principalFactory;
            _currentUserContext = currentUserContext;
        }

        public void SetupAuthenticationContext()
        {
            var identity = _securityContext.CurrentIdentity;

            var principal = _principalFactory.CreatePrincipal(identity);
            
            _securityContext.CurrentUser = principal;
            _currentUserContext.SetUser(principal);
        }
    }
}