using Dovetail.SDK.Bootstrap.Clarify;
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

            For<IClarifySession>().Use(ctx => ctx.GetInstance<IClarifySessionCache>().GetUserSession());

            IncludeRegistry<BootstrapRegistry>();
            IncludeRegistry<ModelMapperRegistry>();
        }
    }
}