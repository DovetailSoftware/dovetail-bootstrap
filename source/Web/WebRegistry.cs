using Dovetail.SDK.Bootstrap;
using StructureMap.Configuration.DSL;

namespace Bootstrap.Web
{
    public class WebRegistry : Registry
    {
        public WebRegistry()
        {
            IncludeRegistry<BootstrapRegistry>();
        }
    }
}