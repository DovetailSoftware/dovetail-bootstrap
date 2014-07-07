using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Foundation;
using NUnit.Framework;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration.Session
{
	[TestFixture]
	public class impersonation_context
	{
		protected IContainer _container;
		protected UserImpersonationService _cut;
		protected DovetailDatabaseSettings _settings;

		[TestFixtureSetUp]
		public void BeforeAll()
		{
			//log4net.Config.XmlConfigurator.Configure(new FileInfo("bootstrap.log4net"));

			_container = bootstrap_ioc.getContainer(c => { });
			_cut = _container.GetInstance<UserImpersonationService>();

			_settings = _container.GetInstance<DovetailDatabaseSettings>();
			_settings.IsImpersonationEnabled = true;

			beforeAll();
		}

		public virtual void beforeAll() { }
	}


	public class user_impersonation_service 
	{
		public class when_setting_is_disabled : impersonation_context
		{
			[Test]
			public void should_do_nothing()
			{
				_settings.IsImpersonationEnabled = false;

				_cut.CancelImpersonation("annie").ShouldEqual(-1);
				_cut.CreateImpersonation("annie", "hank").ShouldEqual(-1);
				_cut.GetImpersonatedLoginFor("annie").ShouldBeNull();
			}
		}

		public class get_impersonated_login_for : impersonation_context
		{
			[Test]
			public void when_not_impersonating_should_return_null()
			{
				UserImpersonationService.cancelImpersonationFor("annie");

				_cut.GetImpersonatedLoginFor("annie").ShouldBeNull();
			}

			[Test]
			public void when_impersonating_should_return_impersonated_user()
			{
				UserImpersonationService.createImpersonationFor("annie", "hank");

				_cut.GetImpersonatedLoginFor("annie").ShouldEqual("hank");
			}
		}

		public class canceling_impersonation : impersonation_context
		{
			[SetUp]
			public void beforeEach()
			{
				UserImpersonationService.createImpersonationFor("annie", "hank");
			}

			[Test]
			public void when_input_is_empty_does_nothing()
			{
				_cut.CancelImpersonation("").ShouldEqual(-1); ;
			}

			[Test]
			public void should_remove_proxy_user_relation()
			{
				_cut.CancelImpersonation("annie");

				_cut.GetImpersonatedLoginFor("annie").ShouldBeNull();
			}

			[Test]
			public void should_create_act_entry()
			{
				var actEntryObjid = _cut.CancelImpersonation("annie");

				var dataSet = _container.GetInstance<IApplicationClarifySession>().CreateDataSet();
				var actEntryGeneric = dataSet.CreateGeneric("act_entry");
				actEntryGeneric.Filter(f => f.Equals("objid", actEntryObjid));
				actEntryGeneric.IncludeRelations = true;
				actEntryGeneric.Query();

				var entry = actEntryGeneric.DataRows().First();
				entry.AsString("proxy").ShouldEqual("hank");
				entry.AsString("addnl_info").ShouldContain("Revert impersonation of hank");
				var annieUserId = _container.GetInstance<IClarifySessionCache>().GetSession("annie").SessionUserID;
				entry.AsInt("act_entry2user").ShouldEqual(annieUserId);
				entry.AsInt("act_code").ShouldEqual(94003);
			}
		}

		public class create_impersonation : impersonation_context
		{
			[SetUp]
			public void beforeEach()
			{
				UserImpersonationService.cancelImpersonationFor("annie");
			}

			[Test]
			public void when_logins_are_unknown_should_throw_exception()
			{
				typeof(ArgumentException).ShouldBeThrownBy(() => _cut.CreateImpersonation("unknown", "hank"));
				typeof(ArgumentException).ShouldBeThrownBy(() => _cut.CreateImpersonation("annie", "unknown"));
			}

			[Test]
			public void should_add_proxy_user_relation()
			{
				_cut.CreateImpersonation("annie", "hank");

				_cut.GetImpersonatedLoginFor("annie").ShouldEqual("hank");
			}

			[Test]
			public void should_create_act_entry()
			{
				var actEntryObjid = _cut.CreateImpersonation("annie", "hank");

				var dataSet = _container.GetInstance<IApplicationClarifySession>().CreateDataSet();
				var actEntryGeneric = dataSet.CreateGeneric("act_entry");
				actEntryGeneric.Filter(f => f.Equals("objid", actEntryObjid));
				actEntryGeneric.IncludeRelations = true;
				actEntryGeneric.Query();

				var entry = actEntryGeneric.DataRows().First();
				entry.AsString("proxy").ShouldEqual("hank");
				entry.AsString("addnl_info").ShouldContain("Impersonate hank");
				var annieUserId = _container.GetInstance<IClarifySessionCache>().GetSession("annie").SessionUserID;
				entry.AsInt("act_entry2user").ShouldEqual(annieUserId);
				entry.AsInt("act_code").ShouldEqual(94002);
			}
		}
	}
}
