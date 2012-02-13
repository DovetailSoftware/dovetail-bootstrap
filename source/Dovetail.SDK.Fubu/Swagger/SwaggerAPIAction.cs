using System.Linq;
using FubuMVC.Core;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class SwaggerAPIActionRequest
    {
        [RouteInput]
        public string GroupKey { get; set; }
    }

    public class SwaggerAPIAction
    {
        private readonly ApiFinder _apiActionFinder;

        public SwaggerAPIAction(ApiFinder apiActionFinder)
        {
            _apiActionFinder = apiActionFinder;
        }

        //[AsymmetricJson]
        public Resource Execute(SwaggerAPIActionRequest request)
        {
            //find all IApi resulting calls
            //group all actions by pattern first part past api : /api/{group}/


            var apis = _apiActionFinder
                .ActionsForGroup(request.GroupKey)
                .Select(s =>
                            {
                                var pattern = s.ParentChain().Route.Pattern;

                                //TODO make this detail come from attribute?
                                var description = "Something pithy about this resource api.";

                                return new API { path = pattern, description = description };
                            }).ToArray();

            return new Resource
                       {
                           //TODO make this an absolute URL
                           basePath = SwaggerHelperKillItWithFire.GetAPIResourcePattern().Replace("{GroupKey}", request.GroupKey),
                           apiVersion = "0.2",
                           swaggerVersion = "1.0",
                           apis = apis
                       };
        }
    }
}