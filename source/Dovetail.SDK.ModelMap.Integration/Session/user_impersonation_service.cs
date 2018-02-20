using System;
using System.Linq;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Clarify.Extensions;
using FChoice.Common.Data;
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

				_cut.StopImpersonating("annie").ShouldEqual(-1);
				_cut.StartImpersonation("annie", "hank").ShouldEqual(-1);
				_cut.GetImpersonatedLoginFor("annie").ShouldBeNull();
			}
		}

		public class scenario : impersonation_context
		{
			[SetUp]
			public void beforeEach()
			{
				UserImpersonationService.cancelImpersonationFor("annie");
				setAllowProxy("hank", true);
				setActiveStatus("hank", true);
			}

			[Test]
			public void when_impersonating_should_return_impersonated_user()
			{
				_cut.StartImpersonation("annie", "hank");

				var impersonated = _container.GetInstance<ICurrentSDKUser>();
				impersonated.SetUser("annie");
				impersonated.Username.ShouldEqual("hank");
				impersonated.ImpersonatingUsername.ShouldEqual("annie");

				_cut.StopImpersonating("annie");

				var normal = _container.GetInstance<ICurrentSDKUser>();
				normal.SetUser("annie");
				normal.Username.ToLowerInvariant().ShouldEqual("annie");
				normal.ImpersonatingUsername.ShouldBeNull();
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
				setActiveStatus("hank", true);

				UserImpersonationService.createImpersonationFor("annie", "hank");

				_cut.GetImpersonatedLoginFor("annie").ShouldEqual("hank");
			}
		}

		public class stop_impersonating : impersonation_context
		{
			[SetUp]
			public void beforeEach()
			{
				setActiveStatus("hank", true);
				UserImpersonationService.createImpersonationFor("annie", "hank");
			}

			[Test]
			public void when_input_is_empty_does_nothing()
			{
				_cut.StopImpersonating("").ShouldEqual(-1); ;
			}

			[Test]
			public void should_remove_proxy_user_relation()
			{
				_cut.StopImpersonating("annie");

				_cut.GetImpersonatedLoginFor("annie").ShouldBeNull();
			}

			[Test]
			public void should_create_act_entry()
			{
				var actEntryObjid = _cut.StopImpersonating("annie");

				var dataSet = _container.GetInstance<IApplicationClarifySession>().CreateDataSet();
				var actEntryGeneric = dataSet.CreateGeneric("act_entry");
				actEntryGeneric.Filter(f => f.Equals("objid", actEntryObjid));
				actEntryGeneric.IncludeRelations = true;
				actEntryGeneric.Query();

				var entry = actEntryGeneric.DataRows().First();
				entry.AsString("addnl_info").ShouldContain("Revert impersonation of hank");
				var annieUserId = _container.GetInstance<IClarifySessionCache>().GetSession("annie").SessionUserID;
				entry.AsInt("act_entry2user").ShouldEqual(annieUserId);
				entry.AsInt("act_code").ShouldEqual(94003);
			}
		}

		public class start_impersonation : impersonation_context
		{
			[SetUp]
			public void beforeEach()
			{
				UserImpersonationService.cancelImpersonationFor("annie");
				setAllowProxy("hank", true);
				setActiveStatus("hank", true);
			}

			[Test]
			public void when_logins_are_unknown_should_throw_exception()
			{
				typeof(ArgumentException).ShouldBeThrownBy(() => _cut.StartImpersonation("unknown", "hank"))
					.Message.ShouldContain("impersonating user unknown does not exist");
				typeof(ArgumentException).ShouldBeThrownBy(() => _cut.StartImpersonation("annie", "unknown"))
					.Message.ShouldContain("user being impersonated unknown does not exist");
			}

			[Test]
			public void when_impersonated_user_does_not_allow_proxies_should_throw_exception()
			{
				setAllowProxy("hank", false);

				typeof(ArgumentException).ShouldBeThrownBy(() => _cut.StartImpersonation("annie", "hank"))
					.Message.ShouldContain("does not allow others to impersonate");
			}

			[Test]
			public void should_add_proxy_user_relation()
			{
				_cut.StartImpersonation("annie", "hank");

				_cut.GetImpersonatedLoginFor("annie").ShouldEqual("hank");
			}

			[Test]
			public void should_create_act_entry()
			{
				var actEntryObjid = _cut.StartImpersonation("annie", "hank");

				var dataSet = _container.GetInstance<IApplicationClarifySession>().CreateDataSet();
				var actEntryGeneric = dataSet.CreateGeneric("act_entry");
				actEntryGeneric.Filter(f => f.Equals("objid", actEntryObjid));
				actEntryGeneric.IncludeRelations = true;
				actEntryGeneric.Query();

				var entry = actEntryGeneric.DataRows().First();
				entry.AsString("addnl_info").ShouldContain("Impersonate hank");
				var annieUserId = _container.GetInstance<IClarifySessionCache>().GetSession("annie").SessionUserID;
				entry.AsInt("act_entry2user").ShouldEqual(annieUserId);
				entry.AsInt("act_code").ShouldEqual(94002);
			}
		}

		public class stop_impersonation_if_user_is_inactive : impersonation_context
		{
			[SetUp]
			public void beforeEach()
			{
				UserImpersonationService.cancelImpersonationFor("annie");
				setAllowProxy("hank", true);
				setActiveStatus("hank", true);
			}

			[Test]
			public void should_return_null_for_impersonated_user_if_inactive()
			{
				_cut.StartImpersonation("annie", "hank");
				_cut.GetImpersonatedLoginFor("annie").ShouldEqual("hank");

				setActiveStatus("hank", false);

				_cut.GetImpersonatedLoginFor("annie").ShouldBeNull();
			}
		}

		public static void setAllowProxy(string userLogin, bool canProxy)
		{
			var sql = new SqlHelper("update table_employee set allow_proxy = {0} where objid = (select objid from table_user where login_name = {1})");
			sql.Parameters.Add("canProxy", canProxy ? 1 : 0);
			sql.Parameters.Add("login", userLogin);
			sql.ExecuteNonQuery();
		}

		public static void setActiveStatus(string userLogin, bool isActive)
		{
			var sql = new SqlHelper("update table_user set status = {0} where login_name = {1}");
			sql.Parameters.Add("isActive", isActive ? 1 : 0);
			sql.Parameters.Add("login", userLogin);
			sql.ExecuteNonQuery();
		}
	}
}
