using System;
using Dovetail.SDK.Bootstrap;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using NUnit.Framework;
using Rhino.Mocks;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration.Session
{
	public abstract class session_cache
	{
		[TestFixture]
		public class session_cache_context
		{
			protected IContainer _container;
			protected ClarifySessionCache _cut;
			protected IUserClarifySessionConfigurator _userClarifySessionConfigurator;
			protected ClarifySession _expectedSession;
			protected IClarifyApplication _clarifyApplication;
			protected DovetailDatabaseSettings _settings;
			protected IUserSessionStartObserver _userSessionStartObserver;
			protected IUserSessionEndObserver _userSessionEndObserver;

			[TestFixtureSetUp]
			public void beforeAll()
			{
				_userClarifySessionConfigurator = MockRepository.GenerateStub<IUserClarifySessionConfigurator>();
				_clarifyApplication = MockRepository.GenerateStub<IClarifyApplication>();

				_userSessionStartObserver = MockRepository.GenerateStub<IUserSessionStartObserver>();
				_userSessionEndObserver = MockRepository.GenerateStub<IUserSessionEndObserver>();

				_container = bootstrap_ioc.getContainer(c =>
				{
					c.For<IUserClarifySessionConfigurator>().Use(_userClarifySessionConfigurator);
					c.For<IClarifyApplication>().Use(_clarifyApplication);
					c.For<IUserSessionStartObserver>().Use(_userSessionStartObserver);
					c.For<IUserSessionEndObserver>().Use(_userSessionEndObserver);
				});

				_cut = _container.GetInstance<ClarifySessionCache>();

				_settings = _container.GetInstance<DovetailDatabaseSettings>();

				//have to create a REAL session because a test fake just won't do
				_expectedSession = CreateRealSession();
			}

			[TestFixtureTearDown]
			public void afterAll()
			{
				_expectedSession.CloseSession();
			}

			protected ClarifySession CreateRealSession()
			{
				return new ClarifyApplicationFactory(_settings, MockRepository.GenerateStub<ILogger>()).Create().CreateSession();
			}

			[SetUp]
			public void beforeEach()
			{
				setup();
			}

			public virtual void setup() {}
		}

		public class session_user : session_cache_context
		{
			private const string UserName = "annie";

			public override void setup()
			{
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_expectedSession).Repeat.Once();
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(true);
			}
			[Test]
			public void should_have_expected_id()
			{
				_cut.GetSession(UserName).Id.ShouldEqual(_expectedSession.SessionID);
			}

			[Test]
			public void should_be_created_once()
			{
				var result = _cut.GetSession(UserName);
					
				_cut.GetSession(UserName).ShouldBeTheSameAs(result);
			}

			[Test]
			public void should_be_configured()
			{
				_cut.GetSession(UserName);

				_userClarifySessionConfigurator.AssertWasCalled(a => a.Configure(_expectedSession));
			}

			[Test]
			public void should_tell_observer()
			{
				_cut.GetSession(UserName);

				_userSessionStartObserver.AssertWasCalled(a => a.SessionStarted(null), x=>x.IgnoreArguments());
			}
		}

		public class ejecting_a_session_user : session_cache_context
		{
			private ClarifySession _secondSession;
			private IClarifySession _result;

			
			private const string UserName = "annie";

			public override void setup()
			{
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_expectedSession).Repeat.Once();
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(true).Repeat.Once();
				
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(false).Repeat.Once();

				_secondSession = CreateRealSession();
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_secondSession).Repeat.Once();
				_clarifyApplication.Stub(s => s.IsSessionValid(_secondSession.SessionID)).Return(true);

				_result = _cut.GetSession(UserName);
			}

			[Test]
			public void should_return_true()
			{
				_cut.EjectSession(UserName).ShouldBeTrue();
			}

			[Test]
			public void should_force_a_new_session_to_be_created()
			{
				_result.Id.ShouldEqual(_expectedSession.SessionID);
				_cut.EjectSession(UserName);
				var newSession = _cut.GetSession(UserName);
				newSession.Id.ShouldEqual(_secondSession.SessionID); ;
			}

			[Test]
			public void should_tell_observer()
			{
				_cut.EjectSession(UserName);

				_userSessionEndObserver.AssertWasCalled(a => a.SessionExpired(_result), x => x.IgnoreArguments());
			}
		}


		public class ejecting_a_session_that_does_not_exist : session_cache_context
		{
			private const string UserName = "annie";

			[Test]
			public void should_return_false()
			{
				_cut.EjectSession(UserName).ShouldBeFalse();
			}

			[Test]
			public void should_not_tell_observer()
			{
				_cut.EjectSession(UserName);

				_userSessionEndObserver.AssertWasNotCalled(a => a.SessionExpired(null), x => x.IgnoreArguments());
			}
		}

		public class session_user_with_failed_attempts : session_cache_context
		{
			private const string UserName = "annie";
			
			public override void setup()
			{
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_expectedSession);
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(false);
			}

			[Test]
			public void should_throw_an_exception()
			{
				typeof(ApplicationException).ShouldBeThrownBy(()=>_cut.GetSession(UserName));
			}
		}

		public class application_session_user : session_cache_context
		{
			protected IClarifySession _result;

			public override void  setup()
			{
				_clarifyApplication.Expect(s => s.CreateSession(_settings.ApplicationUsername, ClarifyLoginType.User)).Return(_expectedSession);
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(true);
				_result = _cut.GetApplicationSession();
			}

			[Test]
			public void should_have_expected_id()
			{
				_cut.GetApplicationSession().Id.ShouldEqual(_expectedSession.SessionID);
			}

			[Test]
			public void should_be_created_once()
			{
				_cut.GetApplicationSession().ShouldBeTheSameAs(_result);
			}

			[Test]
			public void should_use_application_username()
			{
				_clarifyApplication.VerifyAllExpectations();
			}

			[Test]
			public void should_not_be_configured()
			{
				_userClarifySessionConfigurator.AssertWasNotCalled(a=>a.Configure(_expectedSession));
			}

			[Test]
			public void ejecting_session_should_not_tell_observer()
			{
				var result = _cut.EjectSession(_settings.ApplicationUsername);

				result.ShouldBeTrue();

				_userSessionEndObserver.AssertWasNotCalled(a => a.SessionExpired(null), x => x.IgnoreArguments());
			}
		}
	}
}