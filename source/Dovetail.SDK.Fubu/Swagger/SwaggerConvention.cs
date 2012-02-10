using System;
using System.Linq;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using Swagger.Net;

namespace FubuMVC.Swagger
{
    public class SwaggerConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.AddActionFor("api", typeof (SwaggerResourceAction));
        }
    }

    public class SwaggerResourceAction
    {
        [AsymmetricJson]
        public Resources Execute()
        {
            var apis = new[] {new ResourceAPI {description = "wee", path = "wee.{format}"}};
            return new Resources {basePath = "full url", apiVersion = "0.2", swaggerVersion = "1.0", apis = apis};
        }
    }

    public class SwaggerDocsPolicy : IConfigurationAction
    {
        private readonly Type _apiMarker;

        public SwaggerDocsPolicy(Type apiMarker)
        {
            _apiMarker = apiMarker;
        }

        public void Configure(BehaviorGraph graph)
        {
            var apiActions = graph.Actions().Where(a => a.InputType().CanBeCastTo(_apiMarker));
        }
    }
}
