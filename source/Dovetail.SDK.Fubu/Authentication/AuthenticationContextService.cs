using Dovetail.SDK.Bootstrap.Clarify;
using FubuMVC.Core.Security;

namespace Dovetail.SDK.Fubu.Authentication
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

            _securityContext.CurrentUser = _principalFactory.CreatePrincipal(identity);

            _currentUserContext.SetUserName(identity.Name);
        }
    }
}