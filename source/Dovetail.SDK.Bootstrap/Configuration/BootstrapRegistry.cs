using System.Web;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.History.AssemblerPolicies;
using Dovetail.SDK.Bootstrap.History.Configuration;
using Dovetail.SDK.Bootstrap.History.TemplatePolicies;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap.Configuration
{
    public class BootstrapRegistry : Registry
    {
        public BootstrapRegistry()
        {
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.WithDefaultConventions();
                s.AddAllTypesOf<IHistoryAssemblerPolicy>();
            });

            IncludeRegistry<AppSettingProviderRegistry>();

            For<IClarifyApplicationFactory>().Singleton().Use<ClarifyApplicationFactory>();

            //configure the container to use the session cache as a factory for the current user's session
            //any web class that takes a dependency on IClarifySession will get a session for the current 
            //authenticated user. 
            For<IClarifySessionCache>().Singleton().Use<ClarifySessionCache>();
            For<IClarifySession>().Use(ctx => ctx.GetInstance<IClarifySessionCache>().GetUserSession());
            For<IApplicationClarifySession>().Singleton().Use(ctx => ctx.GetInstance<IApplicationSessionCache>().GetApplicationSession());

            For<IListCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().ListCache);
            For<ISchemaCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().SchemaCache);
            For<IStringCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().StringCache);
            For<ILocaleCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().LocaleCache);
            For<IListCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().ListCache);


            //pulled in from FubuMVC for when authentication wraps HttpContext
            For<HttpContextBase>().Use<HttpContextWrapper>()
                .Ctor<HttpContext>().Is(x => x.ConstructedBy(BuildContextWrapper));

            For<ILogger>()
                .AlwaysUnique()
                .Use(s => s.ParentType == null ? new Log4NetLogger(s.BuildStack.Current.ConcreteType) : new Log4NetLogger(s.ParentType));


            //It is the responsibility of the application using bootstrap to set the current sdk user's login 
            For<ICurrentSDKUser>().HybridHttpOrThreadLocalScoped().Use<CurrentSDKUser>();

            this.ActEntryTemplatePolicies<DefaultActEntryTemplatePolicyRegistry>();
        }

        public HttpContext BuildContextWrapper()
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current;
                }
            }
            catch (HttpException)
            {
                //This is only here for web startup when HttpContext.Current is not available.
            }

            return null;
        }
    }
}