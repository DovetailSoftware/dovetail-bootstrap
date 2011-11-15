using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Clarify;
using StructureMap.Configuration.DSL;

namespace Bootstrap.Web
{
    public class WebRegistry : Registry
    {
        public WebRegistry()
        {
            IncludeRegistry<BootstrapRegistry>();

            For<ICurrentSDKUser>().HybridHttpOrThreadLocalScoped().Use(ctx =>
                                                                           {
                                                                               return new CurrentSDKUser { Username = "sa" };
                                                                           });
        }
    }
}