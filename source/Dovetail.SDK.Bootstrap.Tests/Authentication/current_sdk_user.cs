using System.Linq;
using System.Security.Principal;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.DataObjects;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests.Authentication
{
	public class current_sdk_user
	{
		[TestFixture]
		public class authenticated_user : Context<CurrentSDKUser>
		{
			private ITimeZone _sdkUserTimeZone;
			private string _username;
			private SDKUser _sdkUser;


			public override void Given()
			{
				_sdkUserTimeZone = MockFor<ITimeZone>();

				var permissions = new[] {"permission1", "permission2"};
				_username = "username";
				_cut.SetUser(new DovetailPrincipal(new GenericIdentity(_username), permissions));
				
				_sdkUser = new SDKUser {FirstName = "first", LastName = "last", Queues = new [] {new SDKUserQueue {Name = "queue1"}}, Timezone = _sdkUserTimeZone, Login = "user login", Workgroup = "user workgroup"};
				MockFor<IUserDataAccess>().Stub(s => s.GetUser(_username)).Return(_sdkUser);
			}

			[Test]
			public void should_be_authenticated()
			{
				_cut.IsAuthenticated.ShouldBeTrue();
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
				_cut.Queues.ShouldHaveMatchingContents(_sdkUser.Queues);
			}

			[Test]
			public void workgroup_is_based_on_sdk_user_model()
			{
				_cut.Workgroup.ShouldEqual(_sdkUser.Workgroup);
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
			public void username_is_the_application_user()
			{
				_cut.Username.ShouldEqual(_settings.ApplicationUsername);
			}
		}

		[TestFixture]
		public class non_authenticated_user_defaults : Context<CurrentSDKUser>
		{
			private DovetailDatabaseSettings _settings;
			private ITimeZone _defaultTimeZone;

			public override void OverrideMocks()
			{
				_settings = new DovetailDatabaseSettings {ApplicationUsername = "app user name"};
				Override(_settings);
			}

			public override void Given()
			{
				_defaultTimeZone = MockFor<ITimeZone>();
				MockFor<ILocaleCache>().Stub(s => s.ServerTimeZone).Return(_defaultTimeZone);
			}

			[Test]
			public void should_not_be_authenticated()
			{
				_cut.IsAuthenticated.ShouldBeFalse();
			}

			[Test]
			public void username_is_the_application_user()
			{
				_cut.Username.ShouldEqual(_settings.ApplicationUsername);
			}

			[Test]
			public void should_be_the_server_timezone()
			{
				_cut.Timezone.ShouldEqual(_defaultTimeZone);
			}

			[Test]
			public void fullname_is_empty()
			{
				_cut.Fullname.ShouldBeEmpty();
			}

			[Test]
			public void queues_are_empty()
			{
				_cut.Queues.Any().ShouldBeFalse();
			}

			[Test]
			public void workgroup_is_empty()
			{
				_cut.Workgroup.ShouldBeEmpty();
			}
		}

		//public static ICurrentSDKUser createCurrentUserWithSession(IClarifySession session, IContainer container)
		//{
		//    var currentUser = container.GetInstance<ICurrentUser>();
		//    currentUser.ClarifySession = session;

		//    return currentUser;
		//}

		//[TestFixture]
		//public class queue_membership : Context<CurrentSDKUser>
		//{
		//    private const int _userObjid = 1234;
		//    private readonly IEnumerable<SDKUserQueue> _expectedQueues = new[] { new SDKUserQueue { Name = "queue1" }, new SDKUserQueue { Name = "queue2" } };
		//    private IEnumerable<SDKUserQueue> _queues;

		//    public override void Given()
		//    {
		//        var clarifySession = MockFor<IClarifySession>();
		//        clarifySession.Stub(s => s.SessionUserID).Return(_userObjid);
		//        _cut.ClarifySession = clarifySession;

		//        MockFor<IEmployeeDataAccess>().Stub(a => a.GetQueueMemberships(Arg<int>.Is.Anything)).Return(_expectedQueues).Repeat.Once();

		//        _queues = _cut.QueueMemberships;
		//    }

		//    [Test]
		//    public void should_be_loaded_from_employee_dataaccess()
		//    {
		//        MockFor<IEmployeeDataAccess>().AssertWasCalled(a => a.GetQueueMemberships(_userObjid));
		//    }

		//    [Test]
		//    public void should_return_all_queues()
		//    {
		//        _queues.ShouldBeTheSameAs(_expectedQueues);
		//    }

		//    [Test]
		//    public void should_return_same_queue_enumerable_evertime()
		//    {
		//        var queues = _cut.QueueMemberships;

		//        queues.ShouldBeTheSameAs(_expectedQueues);
		//    }
		//}


		//    [TestFixture]
		//    public class workgroup : Context<CurrentUser>
		//    {
		//        private string _workgroup;
		//        private const string expectedWorkgroupName = "expectedWorkgroupName";
		//        private const int _employeeObjid = 1234;

		//        public override void Given()
		//        {
		//            var clarifySession = MockFor<IClarifySession>();
		//            clarifySession.Stub(s => s.SessionEmployeeID).Return(_employeeObjid);
		//            _cut.ClarifySession = clarifySession;

		//            MockFor<IEmployeeDataAccess>().Stub(a => a.GetWorkgroup(Arg<int>.Is.Anything)).Return(expectedWorkgroupName).Repeat.Once();

		//            _workgroup = _cut.Workgroup;
		//        }

		//        [Test]
		//        public void should_be_loaded_from_employee_dataaccess()
		//        {
		//            MockFor<IEmployeeDataAccess>().AssertWasCalled(a => a.GetWorkgroup(_employeeObjid));
		//        }

		//        [Test]
		//        public void should_return_workgroup()
		//        {
		//            _workgroup.ShouldEqual(expectedWorkgroupName);
		//        }

		//        [Test]
		//        public void should_return_same_workgroup()
		//        {
		//            var workgroup = _cut.Workgroup;

		//            workgroup.ShouldEqual(expectedWorkgroupName);
		//        }
		//    }

		//    [TestFixture]
		//    public class current_user_integration : ContainerFixture<CurrentUser>
		//    {
		//        protected override void beforeEach()
		//        {
		//            _cut.ClarifySession = AdministratorClarifySession;
		//        }

		//        [Test]
		//        public void should_return_queues_the_user_is_a_member_of()
		//        {
		//            var queueMemberships = _cut.QueueMemberships;

		//            //queueMemberships.Each(e => Console.WriteLine(e.Name));

		//            queueMemberships.ShouldNotBeEmpty();
		//        }
		//    }
		//}
	}
}