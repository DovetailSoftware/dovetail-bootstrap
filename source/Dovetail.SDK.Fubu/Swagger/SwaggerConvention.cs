using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Dovetail.SDK.Bootstrap;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class SwaggerConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            graph.AddActionFor("api", typeof (SwaggerResourceAction))
                .Calls.ToList().Each(call => call.ForAttributes<ModifyChainAttribute>(att => att.Alter(call))); ;
        }
    }

    public class SwaggerResourceAction
    {
        private readonly BehaviorGraph _graph;

        public SwaggerResourceAction(BehaviorGraph graph)
        {
            _graph = graph;
        }

        [AsymmetricJson]
        public ResourceDiscovery Execute()
        {
            //find all IApi resulting calls

            var apiActions = _graph.Actions().Where(a => a.InputType().CanBeCastTo<IApi>()); //make action finding configurable

            //group all actions by pattern first part past api : /api/{group}/

            var partGroupExpression = new Regex(@"^/?api/(?<group>[a-zA-Z0-9_\-\.\+]+)/?");
            var routeGroups = apiActions
                .Select(s => new {s.ParentChain().Route, Action = s})
                .GroupBy(a =>
                             {
                                 var match = partGroupExpression.Match(a.Route.Pattern);
                                 return match.Success ? match.Groups["group"].Value : null;
                             });

            //also while we have the routeGroups tuple around we should register routes for each "resource"
            
            var apis = routeGroups.Select(s => new ResourceAPI {path = "/api/" + s.Key + "/"});
            
            //TODO get full server URL for basePath
            var basePath = "/api";
            
            return new ResourceDiscovery {basePath = basePath, apiVersion = "0.2", swaggerVersion = "1.0", apis = apis.ToArray()};
        }
    }
}
