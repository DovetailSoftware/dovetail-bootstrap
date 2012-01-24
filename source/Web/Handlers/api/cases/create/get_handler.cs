using Dovetail.SDK.Bootstrap.Clarify;

namespace Bootstrap.Web.Handlers.api.cases.create
{
    public class get_handler
    {
        private readonly ICurrentSDKUser _currentSdkUser;

        public get_handler(ICurrentSDKUser currentSdkUser)
        {
            _currentSdkUser = currentSdkUser;
        }

        public CreateCaseModel Execute(CreateCaseRequest request)
        {
            return new CreateCaseModel
                       {
                           UserIsAuthenticated = _currentSdkUser.IsAuthenticated
                       };
        }
    }
    
    public class CreateCaseRequest
    {
    }
}