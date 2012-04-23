using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.ModelMap;

namespace Bootstrap.Web.Handlers.home
{
    public class get_handler
    {
        private readonly IModelBuilder<UserOpenCaseListing> _listingBuilder;
        private readonly ICurrentSDKUser _currentUser;

        public get_handler(IModelBuilder<UserOpenCaseListing> listingBuilder, ICurrentSDKUser currentUser)
        {
            _listingBuilder = listingBuilder;
            _currentUser = currentUser;
        }

        public HomeModel Execute(HomeRequest request)
        {
            var userCases = _listingBuilder.GetAll();

            return new HomeModel
                {
                    Cases = userCases,
                    Queues = _currentUser.Queues.ToArray()
                };
        }
    }

    public class HomeRequest { }

    public class HomeModel 
    {
        public UserOpenCaseListing[] Cases { get; set; }
        public SDKUserQueue[] Queues{ get; set; }
    }
}