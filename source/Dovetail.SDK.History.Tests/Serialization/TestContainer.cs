using System;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Bootstrap.History.AssemblerPolicies;
using Dovetail.SDK.ModelMap.Configuration;
using Dovetail.SDK.ModelMap.Legacy;
using Dovetail.SDK.ModelMap.Legacy.Registration;
using FChoice.Foundation.Clarify;
using StructureMap;
using StructureMap.Graph;

namespace Dovetail.SDK.History.Tests.Serialization
{
	public static class TestContainer
	{
		public static IContainer getContainer()
		{
			return getContainer(_ => { });
		}

		public static IContainer getContainer(Action<ConfigurationExpression> configAction)
		{
			var container = new Container(c =>
			{
				c.Scan(s =>
				{
					s.TheCallingAssembly();
					s.AddAllTypesOf<IHistoryAssemblerPolicy>();
					s.AssemblyContainingType<DovetailDatabaseSettings>();
					s.AssemblyContainingType<DovetailMappingException>();
					s.ConnectImplementationsToTypesClosing(typeof(ModelMap<>));
					s.Convention<SettingsScanner>();
					s.WithDefaultConventions();
				});
				c.For<IClarifyApplicationFactory>().Singleton().Use<ClarifyApplicationFactory>();
				c.For<IClarifyApplication>().Singleton().Use(ctx => ctx.GetInstance<IClarifyApplicationFactory>().Create());
				c.For<ILogger>().AlwaysUnique().Use(s => s.ParentType == null ? new Log4NetLogger(s.RootType) : new Log4NetLogger(s.ParentType));
				c.AddRegistry<SettingsProviderRegistry>();
				c.AddRegistry<BootstrapRegistry>();
				c.AddRegistry<ModelMapperRegistry>();
				c.AddRegistry<HistoryRegistry>();

				if (configAction != null) configAction(c);
			});

			return container;
		}
	}
}