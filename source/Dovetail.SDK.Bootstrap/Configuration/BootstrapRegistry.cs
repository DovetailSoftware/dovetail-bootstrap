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
            For<IListCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().ListCache);
            For<ISchemaCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().SchemaCache);
            For<IStringCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().StringCache);
            For<ILocaleCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().LocaleCache);
            For<IListCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().ListCache);

            For<IApplicationClarifySession>().Use(ctx => ctx.GetInstance<IApplicationSessionCache>().GetApplicationSession());

            For<ILogger>()
                .AlwaysUnique()
                .Use(s => s.ParentType == null ? new Log4NetLogger(s.BuildStack.Current.ConcreteType) : new Log4NetLogger(s.ParentType));


            //It is the responsibility of the application using bootstrap to set the current sdk user's login 
            For<ICurrentSDKUser>().HybridHttpOrThreadLocalScoped().Use<CurrentSDKUser>();

            this.ActEntryTemplatePolicies<DefaultActEntryTemplatePolicyRegistry>();
        }
    }
}