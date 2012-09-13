using Dovetail.SDK.Bootstrap.Clarify;
using FubuCore;
using FubuCore.Binding;
using FubuCore.Binding.InMemory;
using FubuCore.Configuration;
using FubuCore.Conversion;
using FubuCore.Reflection;
using StructureMap.Configuration.DSL;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class SettingsProviderRegistry : Registry
	{
		public SettingsProviderRegistry()
		{
			Scan(s =>
				{
					s.AssemblyContainingType<DovetailDatabaseSettings>();
					s.Convention<SettingsScanner>();
				});

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