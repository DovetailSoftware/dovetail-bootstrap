using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.ModelMap;
using Dovetail.SDK.ModelMap.Legacy;

namespace Bootstrap.Web.Handlers.home
{
    public class HomeEndpoint
    {
        private readonly IModelBuilder<UserOpenCaseListing> _listingBuilder;
        private readonly ICurrentSDKUser _currentUser;

        public HomeEndpoint(IModelBuilder<UserOpenCaseListing> listingBuilder, ICurrentSDKUser currentUser)
        {
            _listingBuilder = listingBuilder;
            _currentUser = currentUser;
        }

        public HomeModel Index(HomeRequest request)
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
