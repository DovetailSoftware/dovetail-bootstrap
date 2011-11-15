using System;
using System.Configuration;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Schema;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap
{
    public class BootstrapRegistry : Registry
    {
        public BootstrapRegistry()
        {
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.WithDefaultConventions();

                //TODO uncomment when AppSettingsProvider is no longer broken
                //s.Convention<SettingsScanner>();
            });

            //TODO uncomment when AppSettingsProvider is no longer broken
            //cfg.For<ISettingsProvider>().Use<AppSettingsProvider>(); 

            For<IListCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().ListCache);
            For<ISchemaCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().SchemaCache);
            For<IStringCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().StringCache);
            For<ILocaleCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().LocaleCache);
            For<IListCache>().Use(c => c.GetInstance<IClarifyApplicationFactory>().Create().ListCache);

            //TODO replace this with AppSettingsProvider
            For<DovetailDatabaseSettings>().Use(() =>
                                                    {
                                                        var settings = ConfigurationManager.AppSettings;
                                                        var timeout = settings["DovetailDatabaseSettings.SessionTimeoutInMinutes"];

                                                        return new DovetailDatabaseSettings
                                                                   {
                                                                       ConnectionString = settings["DovetailDatabaseSettings.ConnectionString"],
                                                                       Type = settings["DovetailDatabaseSettings.Type"],
                                                                       SessionTimeoutInMinutes = Convert.ToInt32(timeout)
                                                                   };
                                                    });

            For<IClarifySession>()
                .HybridHttpOrThreadLocalScoped()
                .Use(ctx => ctx.GetInstance<IClarifySessionProvider>().GetHttpRequestSession());

            For<ILogger>()
                .AlwaysUnique()
                .Use(s => s.ParentType == null ? new Log4NetLogger(s.BuildStack.Current.ConcreteType) : new Log4NetLogger(s.ParentType));


            //It is the responsibility of the application using bootstrap to set the current sdk user's login 
            For<ICurrentSDKUser>().HybridHttpOrThreadLocalScoped().Use<CurrentSDKUser>(); 
        }
    }
}