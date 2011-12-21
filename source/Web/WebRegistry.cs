using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.ModelMap.Configuration;
using Dovetail.SDK.ModelMap.Registration;
using StructureMap.Configuration.DSL;

namespace Bootstrap.Web
{
    public class WebRegistry : Registry
    {
        public WebRegistry()
        {
            Scan(scan=>
                     {
                         scan.TheCallingAssembly();
                         scan.ConnectImplementationsToTypesClosing(typeof(ModelMap<>));
                     });

            IncludeRegistry<BootstrapRegistry>();
            IncludeRegistry<ModelMapperRegistry>();
        }
    }
}