using System;
using System.Linq;
using FubuCore;
using FubuMVC.Core.Http;
using FubuMVC.Core.Urls;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class SwaggerResourceDiscoveryAction
    {
        private readonly ApiFinder _apiFinder;
        private readonly IUrlRegistry _urlRegistry;
        private readonly ICurrentHttpRequest _currentHttpRequest;

        public SwaggerResourceDiscoveryAction(ApiFinder apiFinder, IUrlRegistry urlRegistry, ICurrentHttpRequest currentHttpRequest)
        {
            _apiFinder = apiFinder;
            _urlRegistry = urlRegistry;
            _currentHttpRequest = currentHttpRequest;
        }

        //[AsymmetricJson]
        public ResourceDiscovery Execute()
        {
            var baseUrl = _urlRegistry.UrlFor<SwaggerResourceDiscoveryAction>(m => m.Execute());
            var absoluteBaseUrl = _currentHttpRequest.ToFullUrl(baseUrl);

            var apis = _apiFinder
                .ActionsByGroup()
                .Select(s =>
                            {
                                var description = "API for {0}".ToFormat(s.Key);

                                //UGH we need to make relative URLs for swagger to be happy. 
                                var resourceAPIRequestUrl = _urlRegistry.UrlFor(new SwaggerResourceDiscoveryAPIRequest {GroupKey = s.Key});
                                var resourceUrl = baseUrl.UrlRelativeTo(resourceAPIRequestUrl);

                                return new ResourceDiscoveryAPI
                                           {
                                               path = resourceUrl,
                                               description = description
                                           };
                            });

            
            return new ResourceDiscovery
                       {
                           basePath = absoluteBaseUrl,
                           apiVersion = "0.2",
                           swaggerVersion = "1.0",
                           apis = apis.ToArray()
                       };
        }
    }
}