using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuCore.Reflection;
using FubuMVC.Core.Http;
using FubuMVC.Core.Urls;

namespace Dovetail.SDK.Fubu.Swagger
{
    public static class SwaggerExtensions
    {
        public static string GetVersion(this Assembly assembly)
        {
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersion.ProductVersion;
        }

        public static string GetAttribute<T>(this PropertyInfo property, Func<T, string> func) where T : Attribute
        {
            var attribute = property.GetAttribute<T>();
            
            return attribute == null ? String.Empty : func(attribute);
        }

        public static string GetAttribute<T>(this Type type, Func<T, string> func) where T : Attribute
        {
            var attribute = type.GetAttribute<T>();

            return attribute == null ? String.Empty : func(attribute);
        }
    }

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
                                var description = "APIs for {0}".ToFormat(s.Key);

                                //UGH we need to make api urls relative for swagger to be happy. 
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
                           apiVersion = Assembly.GetExecutingAssembly().GetVersion(),
                           swaggerVersion = "1.0",
                           apis = apis.ToArray()
                       };
        }
    }
}