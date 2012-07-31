using System;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Configuration;
using Dovetail.SDK.Bootstrap.History.AssemblerPolicies;
using Dovetail.SDK.Bootstrap.Tests;
using Dovetail.SDK.ModelMap.Registration;
using Dovetail.SDK.ModelMap.Configuration;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration.Session
{
	public class session_cache
	{
		public class clarify_application : Context<ClarifySessionCache>
		{
			[Test]
			public void should_use_factory_once()
			{
				var clarifyApplicationFactory = MockFor<IClarifyApplicationFactory>();
				clarifyApplicationFactory.Expect(e => e.Create()).Repeat.Once();

				var app = _cut.ClarifyApplication;
				app = _cut.ClarifyApplication;

				clarifyApplicationFactory.VerifyAllExpectations();
			}
		}

		public static class bootstrap_ioc
		{
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
						c.For<ILogger>().AlwaysUnique().Use(s => s.ParentType == null ? new Log4NetLogger(s.BuildStack.Current.ConcreteType) : new Log4NetLogger(s.ParentType));
						c.AddRegistry<SettingsProviderRegistry>();
						c.AddRegistry<ModelMapperRegistry>();
						configAction(c);
					});

				return container;
			}
		}

		[TestFixture]
		public class application_session_user 
		{
			private IContainer _container;
			private ClarifySessionCache _cut;
			private MockRepository _mocks;
			private IUserClarifySessionConfigurator _userClarifySessionConfigurator;

			[TestFixtureSetUp]
			public void beforeAll()
			{
				_mocks = new MockRepository();
				_userClarifySessionConfigurator = _mocks.Stub<IUserClarifySessionConfigurator>();

				_container = bootstrap_ioc.getContainer(c=> c.For<IUserClarifySessionConfigurator>().Use(_userClarifySessionConfigurator));

				_cut = _container.GetInstance<ClarifySessionCache>();
			}
			
			[Test]
			public void should_be_created_once()
			{
				var appSession = _cut.GetApplicationSession();

				_cut.GetApplicationSession().ShouldBeTheSameAs(appSession);
			}
		}
	}
}