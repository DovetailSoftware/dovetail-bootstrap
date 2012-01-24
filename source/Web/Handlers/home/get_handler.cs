using System.Collections.Generic;
using Dovetail.SDK.ModelMap;

namespace Bootstrap.Web.Handlers.home
{
    public class get_handler
    {
        private readonly IModelBuilder<UserOpenCaseListing> _listingBuilder;

        public get_handler(IModelBuilder<UserOpenCaseListing> listingBuilder)
        {
            _listingBuilder = listingBuilder;
        }

        public HomeModel Execute(HomeRequest request)
        {
            var userCases = _listingBuilder.GetAll();

            return new HomeModel { Cases = userCases };
        }
    }

    public class HomeRequest { }

    public class HomeModel 
    {
        public IEnumerable<UserOpenCaseListing> Cases { get; set; }
    }
}