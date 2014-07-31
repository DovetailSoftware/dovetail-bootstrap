using System;
using System.IO;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Authentication.Principal;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.ModelMap.Configuration;
using Dovetail.SDK.ModelMap.Registration;
using NUnit.Framework;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration
{
	public class MapFixture
	{
		public IContainer Container { get; private set; }
		public IClarifySession AdministratorClarifySession { get; set; }
		public ICurrentSDKUser CurrentSDKUser { get; set; }

		[TestFixtureSetUp]
		public void FixtureSetup()
		{
			setupLoggingConfigurationWatchFile();

			Container = new Container(cfg =>
			{
				cfg.Scan(x =>
				{
					x.TheCallingAssembly();
					x.AssemblyContainingType<DovetailDatabaseSettings>();
					x.AssemblyContainingType<DovetailMappingException>();
					x.ConnectImplementationsToTypesClosing(typeof (ModelMap<>));
					x.Convention<SettingsScanner>();
					x.WithDefaultConventions();
				});

				cfg.AddRegistry<BootstrapRegistry>();
				cfg.AddRegistry<ModelMapperRegistry>();
				cfg.AddRegistry<SettingsProviderRegistry>();
			});

			AdministratorClarifySession = Container.GetInstance<IApplicationClarifySession>();

			CurrentSDKUser = Container.GetInstance<ICurrentSDKUser>();
			CurrentSDKUser.SetUser("sa");

			beforeAll();
		}

		public virtual void beforeAll()
		{
		}

		private static void setupLoggingConfigurationWatchFile()
		{
			const string loggingConfigFileName = "bootstrap.log4net";
			var loggingConfig = new FileInfo(loggingConfigFileName);
			if (!loggingConfig.Exists)
			{
				loggingConfig = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, loggingConfigFileName));
			}
			log4net.Config.XmlConfigurator.ConfigureAndWatch(loggingConfig);
		}

	}
}