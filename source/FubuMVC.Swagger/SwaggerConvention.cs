using FubuMVC.Core.Registration;
using FubuMVC.Core.Resources.Conneg;

namespace FubuMVC.Swagger
{
    public class SwaggerConvention<T> : IConfigurationAction
    {
        public void Configure(BehaviorGraph graph)
        {
            ////TODO should this route '/api' be configurable?
            //add resource discovery action and force it to return JSON
            graph.AddActionFor("api/resources.json", typeof(SwaggerResourceDiscoveryAction<>), typeof(T)).MakeAsymmetricJson();

            //add resource action and force it to return JSON
            graph.AddActionFor("api/{GroupKey}.json", typeof(SwaggerResourceDiscoveryAPIAction<>), typeof(T)).MakeAsymmetricJson();
        }
    }
}
