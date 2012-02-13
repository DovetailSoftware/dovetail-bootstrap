using FubuMVC.Core.Registration;
using FubuMVC.Core.Resources.Conneg;

namespace Dovetail.SDK.Fubu.Swagger
{
    public class SwaggerConvention : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            ////TODO should this route '/api' be configurable?
            //add resource discovery action and force it to return JSON
            graph.AddActionFor("api/resources.json", typeof(SwaggerResourceDiscoveryAction)).MakeAsymmetricJson();

            //add resource action and force it to return JSON
            graph.AddActionFor("api/{GroupKey}.json", typeof(SwaggerResourceDiscoveryAPIAction)).MakeAsymmetricJson();
        }
    }
}
