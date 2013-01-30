using Dovetail.SDK.Bootstrap.Authentication;
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
            var principalFactory = Container.GetInstance<IPrincipalFactory>();
            CurrentSDKUser.SetUser(principalFactory.CreatePrincipal("sa"));

			beforeAll();
		}

		public virtual void beforeAll() { }

	}
}