using Dovetail.SDK.Bootstrap.Clarify;
using FubuMVC.Core;

namespace Bootstrap.Web.Handlers.user.status
{
    public class get_handler
    {
        private readonly ICurrentSDKUser _currentSdkUser;

        public get_handler(ICurrentSDKUser currentSdkUser)
        {
            _currentSdkUser = currentSdkUser;
        }

        [FubuPartial]
        public UserStatusModel Execute(UserStatusRequest request)
        {
            return new UserStatusModel(_currentSdkUser);
        }
    }

    public class UserStatusRequest { }

    public class UserStatusModel
    {
        public UserStatusModel(ICurrentSDKUser user)
        {
            User = user;
        }

        public ICurrentSDKUser User { get; private set; }
    }
}