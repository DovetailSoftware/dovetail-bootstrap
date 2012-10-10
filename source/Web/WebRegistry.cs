using Dovetail.SDK.Bootstrap.Configuration;
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
                         //web assembly
                         scan.TheCallingAssembly();
                         //bootstrap assembly
						 scan.AssemblyContainingType<WebsiteSettings>();
                         
                         //register model maps found in scanned assemblies
                         //this lets your classes take dependencies on 
                         //IModelBuilder<VIEWMODEL> when doing model projections from the database
                         scan.ConnectImplementationsToTypesClosing(typeof(ModelMap<>));

                         //register any class named similarily to its interface 
                         //effectively contentionally: For<I{classname}>().Use<{classname}>();
                         //e.g. IAuthenticationService is registered to use AuthenticationService;
                         scan.WithDefaultConventions();
                     });
			
			IncludeRegistry<SettingsProviderRegistry>();
            IncludeRegistry<BootstrapRegistry>();
            IncludeRegistry<ModelMapperRegistry>();
        }
    }
}