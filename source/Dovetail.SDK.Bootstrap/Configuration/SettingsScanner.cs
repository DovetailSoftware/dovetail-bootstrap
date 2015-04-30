using System;
using FubuCore.Configuration;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class SettingsScanner : IRegistrationConvention
	{
		public void Process(Type type, Registry graph)
		{
			if (!type.Name.EndsWith("Settings") || type.IsInterface || type.IsAbstract) return;

			graph
				.For(type)
				.LifecycleIs(new SingletonLifecycle())
				.Use(c => c.GetInstance<ISettingsProvider>().SettingsFor(type));
		}
	}
}