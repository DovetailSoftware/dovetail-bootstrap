using Dovetail.SDK.Clarify.Configuration;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using FChoice.Foundation.Schema;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Binding.InMemory;
using FubuCore.Configuration;
using FubuCore.Conversion;
using FubuCore.Reflection;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;

namespace Dovetail.SDK.Clarify
{
    public class ClarifyRegistry : Registry
    {
        public static bool BypassSdkTypes;

        public ClarifyRegistry()
        {
            For<ILogger>()
                .AlwaysUnique()
                .Use(s => s.ParentType == null ? new Log4NetLogger(s.RootType) : new Log4NetLogger(s.ParentType));

            Scan(_ =>
            {
                _.TheCallingAssembly();
                _.AssemblyContainingType<IClarifySession>();
                _.WithDefaultConventions();

                _.AddAllTypesOf<IClarifySessionListener>();
                _.AddAllTypesOf<IStartupPolicy>();
                _.Convention<SettingsScanner>();
            });

            For<IClarifyContext>().Singleton().Use<ClarifyContext>();

            if (!BypassSdkTypes)
            {
                For<ITimeZone>().Use(_ => _.GetInstance<IClarifyContext>().ServerTimeZone);
                For<ILocaleCache>().Singleton().Use(_ => _.GetInstance<IClarifyContext>().LocaleCache);
                For<ISchemaCache>().Singleton().Use(_ => _.GetInstance<IClarifyContext>().SchemaCache);
            }

            For<ITypeDescriptorCache>().Use<TypeDescriptorCache>();
            For<IBindingLogger>().Use<NulloBindingLogger>();
            For<IServiceLocator>().Use<StructureMapServiceLocator>();
            For<IObjectResolver>().Use<ObjectResolver>();
            For<IObjectConverter>().Use<ObjectConverter>();
            For<ISettingsSource>().Add<DovetailAppSettingsSource>();
            For<ISettingsProvider>().Use<SettingsProvider>();
        }
    }
}
