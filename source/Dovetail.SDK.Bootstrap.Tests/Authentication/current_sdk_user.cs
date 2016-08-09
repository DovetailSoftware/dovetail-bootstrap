using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.DataObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests.Authentication
{
	public class current_sdk_user
	{
		[TestFixture]
		public class set_user_gets_details_from_user_data_access : Context<CurrentSDKUser>
		{
			private ITimeZone _sdkUserTimeZone;
			private string _username;
			private SDKUser _sdkUser;

			public override void Given()
			{
				_sdkUserTimeZone = MockFor<ITimeZone>();

				_username = "username";
				_cut.SetUser(_username);

				_sdkUser = new SDKUser
				{
					FirstName = "first",
					LastName = "last",
					Queues = new[] {new SDKUserQueue {Name = "queue1"}},
					Timezone = _sdkUserTimeZone,
					Login = "user login",
					ImpersonatingLogin = "proxy user login",
					PrivClass = "PrivClass",
					Workgroup = "user workgroup"
				};
				MockFor<IUserDataAccess>().Stub(s => s.GetUser(_username)).Return(_sdkUser);
			}

			[Test]
			public void should_be_authenticated()
			{
				_cut.IsAuthenticated.ShouldBeTrue();
			}

			[Test]
			public void username_should_match_login()
			{
				_cut.Username.ShouldEqual(_sdkUser.Login);
			}

			[Test]
			public void proxy_username_should_match_proxy()
			{
				_cut.ImpersonatingUsername.ShouldEqual(_sdkUser.ImpersonatingLogin);
			}

			[Test]
			public void should_be_based_on_sdk_user_model()
			{
				_cut.Timezone.ShouldEqual(_sdkUserTimeZone);
			}

			[Test]
			public void fullname_based_on_sdk_user_model()
			{
				_cut.Fullname.ShouldEqual(_sdkUser.FirstName + " " + _sdkUser.LastName);
			}

			[Test]
			public void queues_are_based_on_sdk_user_model()
			{
				_cut.Queues.ShouldMatch(_sdkUser.Queues);
			}

			[Test]
			public void workgroup_is_based_on_sdk_user_model()
			{
				_cut.Workgroup.ShouldEqual(_sdkUser.Workgroup);
			}

			[Test]
			public void privClass_is_based_on_sdk_user_model()
			{
				_cut.PrivClass.ShouldEqual(_sdkUser.PrivClass);
			}

			[Test]
			public void after_setting_timezone_that_timezone_should_be_used()
			{
				var newTimezone = _services.AddAdditionalMockFor<ITimeZone>();

				_cut.Timezone.ShouldEqual(_sdkUserTimeZone);

				_cut.SetTimezone(newTimezone);

				_cut.Timezone.ShouldEqual(newTimezone);
			}
		}

		[TestFixture]
		public class authenticated_user_signing_off : Context<CurrentSDKUser>
		{
			private DovetailDatabaseSettings _settings;

			public override void OverrideMocks()
			{
				_settings = new DovetailDatabaseSettings { ApplicationUsername = "app user name" };
				Override(_settings);
			}

			public override void Given()
			{
				_cut.SignOut();
			}

			[Test]
			public void should_not_be_authenticated()
			{
				_cut.IsAuthenticated.ShouldBeFalse();
			}

			[Test]
			public void user_details_should_be_retrieved_for_application_user()
			{
				MockFor<IUserDataAccess>().Expect(u => u.GetUser(_settings.ApplicationUsername)).Return(new SDKUser());

				var user = _cut.Username;

				MockFor<IUserDataAccess>().VerifyAllExpectations();
			}
		}
	}
}
