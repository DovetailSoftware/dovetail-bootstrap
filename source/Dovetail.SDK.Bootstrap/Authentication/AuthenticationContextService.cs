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
        private readonly ICurrentSDKUser _currentSdkUser;

        public AuthenticationContextService(ISecurityContext securityContext, IPrincipalFactory principalFactory, ICurrentSDKUser currentSdkUser)
        {
            _securityContext = securityContext;
            _principalFactory = principalFactory;
            _currentSdkUser = currentSdkUser;
        }

        public void SetupAuthenticationContext()
        {
            var identity = _securityContext.CurrentIdentity;

            var principal = _principalFactory.CreatePrincipal(identity);
            
            _securityContext.CurrentUser = principal;
            _currentSdkUser.SetUser(principal);
        }
    }
}