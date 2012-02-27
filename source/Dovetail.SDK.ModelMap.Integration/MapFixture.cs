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
		                                      cfg.AddRegistry<BootstrapRegistry>();
		                                      cfg.AddRegistry<ModelMapperRegistry>();
		                                      cfg.Scan(x =>
		                                                   {
		                                                       x.TheCallingAssembly();
		                                                       x.ConnectImplementationsToTypesClosing(typeof (ModelMap<>));
		                                                   });
		                                  });

            AdministratorClarifySession = Container.GetInstance<IApplicationClarifySession>();

            CurrentSDKUser = Container.GetInstance<ICurrentSDKUser>();
            CurrentSDKUser.SetUser("sa");

			beforeAll();
		}

		public virtual void beforeAll() { }

	}
}