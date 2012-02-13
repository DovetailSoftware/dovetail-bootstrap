using System.Linq;
using FubuMVC.Core;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class SwaggerResourceDiscoveryAPIRequest
    {
        [RouteInput]
        public string GroupKey { get; set; }
    }

    public class SwaggerResourceDiscoveryAPIAction
    {
        private readonly ApiFinder _apiActionFinder;
        private readonly ISwaggerMapper _swaggerMapper;

        public SwaggerResourceDiscoveryAPIAction(ApiFinder apiActionFinder, ISwaggerMapper swaggerMapper)
        {
            _apiActionFinder = apiActionFinder;
            _swaggerMapper = swaggerMapper;
        }

        //[AsymmetricJson]
        public Resource Execute(SwaggerResourceDiscoveryAPIRequest request)
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

                                return new API
                                           {
                                               path = pattern,
                                               description = description,
                                               operations = _swaggerMapper.OperationsFrom(s).ToArray()
                                           };
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