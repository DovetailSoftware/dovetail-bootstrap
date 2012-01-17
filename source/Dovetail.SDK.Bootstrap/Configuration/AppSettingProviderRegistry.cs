using FubuCore.Binding;
using FubuCore.Configuration;
using FubuCore.Reflection;
using Microsoft.Practices.ServiceLocation;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap.Configuration
{
    public class AppSettingProviderRegistry : Registry
    {
        public AppSettingProviderRegistry()
        {
            Scan(s =>
            {
                s.TheCallingAssembly();
                s.WithDefaultConventions();
                s.Convention<SettingsScanner>();
            });

            For<IObjectResolver>().Use<ObjectResolver>();
            For<IServiceLocator>().Use<StructureMapServiceLocator>();
            For<IValueConverterRegistry>().Use<ValueConverterRegistry>();
            For<ITypeDescriptorCache>().Use<TypeDescriptorCache>();
            ForSingletonOf<IPropertyBinderCache>().Use<PropertyBinderCache>();
            ForSingletonOf<ICollectionTypeProvider>().Use<DefaultCollectionTypeProvider>();
            ForSingletonOf<IModelBinderCache>().Use<ModelBinderCache>();
            For<IModelBinder>().Use<StandardModelBinder>();
            For<ISettingsSource>().Add<DovetailAppSettingsSource>();
        }
    }
}