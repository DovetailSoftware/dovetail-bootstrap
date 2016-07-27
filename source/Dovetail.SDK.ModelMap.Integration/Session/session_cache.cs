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
	public class session_cache
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
				//log4net.Config.XmlConfigurator.Configure(new FileInfo("bootstrap.log4net"));

				_userClarifySessionConfigurator = MockRepository.GenerateStub<IUserClarifySessionConfigurator>();
				_clarifyApplication = MockRepository.GenerateStub<IClarifyApplication>();

				_userSessionStartObserver = MockRepository.GenerateStub<IUserSessionStartObserver>();
				_userSessionEndObserver = MockRepository.GenerateStub<IUserSessionEndObserver>();

				_container = bootstrap_ioc.getContainer(c =>{ });
				_container.Configure(c =>
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
				_cut.clear();
			}

			protected ClarifySession CreateRealSession()
			{
				var logger = MockRepository.GenerateStub<ILogger>();
				var metadata = MockRepository.GenerateMock<IWorkflowObjectMetadata>();
				metadata.Stub(s => s.Register()).Return(new WorkflowObjectInfo("info_to_register"));

				return new ClarifyApplicationFactory(_settings, new [] {metadata}, logger).Create().CreateSession();
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
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_expectedSession);
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

			[Test]
			public void refresh_session_context_without_changes()
			{
				var session = _cut.GetSession(UserName);

				_cut.RefreshSession(UserName);

				_cut.GetSession(UserName).ShouldBeTheSameAs(session);
			}
		}

		public class ejecting_a_session_user : session_cache_context
		{
			private IClarifySession _session;
			private bool _result;
			private const string UserName = "annie";

			public override void setup()
			{
				var sessionId = Guid.NewGuid();
				_session = MockRepository.GenerateStub<IClarifySession>();
				_session.Stub(s => s.Id).Return(sessionId);

				_cut.addSessionToCache(UserName, _session);
			}

			[Test]
			public void should_return_true()
			{
				_result = _cut.EjectSession(UserName);

				_result.ShouldBeTrue();
			}

			[Test]
			public void should_close_session()
			{
				_result = _cut.EjectSession(UserName);

				_session.AssertWasCalled(a => a.Close());
			}

			[Test]
			public void should_tell_observer_by_default()
			{
				_result = _cut.EjectSession(UserName, true);

				_userSessionEndObserver.AssertWasCalled(a => a.SessionExpired(_session));
			}

			[Test]
			public void should_not_tell_observer_when_told()
			{
				_result = _cut.EjectSession(UserName, false);

				_userSessionEndObserver.AssertWasNotCalled(a => a.SessionExpired(_session));
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

		public class application_session_user : session_cache_context
		{

			public override void  setup()
			{
				_clarifyApplication.Expect(s => s.CreateSession(_settings.ApplicationUsername, ClarifyLoginType.User)).Return(_expectedSession);
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(true);
			}

			[Test]
			public void should_have_expected_id()
			{
				_cut.GetApplicationSession();

				_cut.GetApplicationSession().Id.ShouldEqual(_expectedSession.SessionID);
			}

			[Test]
			public void should_be_created_once()
			{
				var result = _cut.GetApplicationSession();

				_cut.GetApplicationSession().ShouldBeTheSameAs(result);
			}

			[Test]
			public void should_use_application_username()
			{
				_cut.GetApplicationSession();

				_clarifyApplication.VerifyAllExpectations();
			}

			[Test]
			public void should_be_configured_by_default()
			{
				_cut.GetApplicationSession(true);

				_userClarifySessionConfigurator.AssertWasCalled(a => a.Configure(_expectedSession));
			}
		}

		public class application_session_user_unconfigured : session_cache_context
		{
			[Test]
			public void should_not_be_configured_when_told()
			{
				_clarifyApplication.Expect(s => s.CreateSession(_settings.ApplicationUsername, ClarifyLoginType.User)).Return(_expectedSession);
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(true);
				_cut.GetApplicationSession(false);

				_userClarifySessionConfigurator.AssertWasNotCalled(a => a.Configure(_expectedSession));
			}
		}
	}
}
