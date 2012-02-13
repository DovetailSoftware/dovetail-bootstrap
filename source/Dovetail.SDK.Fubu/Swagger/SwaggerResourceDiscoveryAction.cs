using System.Linq;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class SwaggerResourceDiscoveryAction
    {
        private readonly ApiFinder _apiFinder;

        public SwaggerResourceDiscoveryAction(ApiFinder apiFinder)
        {
            _apiFinder = apiFinder;
        }

        //[AsymmetricJson]
        public ResourceDiscovery Execute()
        {
            var apis = _apiFinder
                .ActionsByGroup()
                .Select(s =>
                            {
                                var description = "Find detail about api group somewhere";
                                return new ResourceDiscoveryAPI
                                           {
                                               path = SwaggerHelperKillItWithFire.GetAPIResourcePattern().Replace("{GroupKey}", s.Key),
                                               description = description
                                           };
                            });

            return new ResourceDiscovery
                       {
                           basePath = SwaggerHelperKillItWithFire.GetBasePathPattern(),
                           apiVersion = "0.2",
                           swaggerVersion = "1.0",
                           DiscoveryApis = apis.ToArray()
                       };
        }
    }
}