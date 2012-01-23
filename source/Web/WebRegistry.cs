using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Fubu.Authentication;
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
                         scan.AssemblyContainingType<IAuthenticationService>();
                         
                         scan.ConnectImplementationsToTypesClosing(typeof(ModelMap<>));
                         scan.WithDefaultConventions();
                     });

            IncludeRegistry<BootstrapRegistry>();
            IncludeRegistry<ModelMapperRegistry>();
        }
    }
}