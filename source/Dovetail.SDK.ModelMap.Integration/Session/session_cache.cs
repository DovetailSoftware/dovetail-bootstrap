using System;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Tests;
using FChoice.Foundation.Clarify;
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

				var app1 = _cut.ClarifyApplication;
				var app2 = _cut.ClarifyApplication;

				app1.ShouldBeTheSameAs(app2);
				clarifyApplicationFactory.VerifyAllExpectations();
			}
		}

		[TestFixture]
		public class session_cache_context
		{
			protected IContainer _container;
			protected ClarifySessionCache _cut;
			protected IUserClarifySessionConfigurator _userClarifySessionConfigurator;
			protected IClarifyApplicationFactory _applicationFactory;
			protected ClarifySession _expectedSession;
			protected IClarifyApplication _clarifyApplication;
			protected DovetailDatabaseSettings _settings;

			[TestFixtureSetUp]
			public void beforeAll()
			{
				_userClarifySessionConfigurator = MockRepository.GenerateStub<IUserClarifySessionConfigurator>();
				_applicationFactory = MockRepository.GenerateStub<IClarifyApplicationFactory>();

				_container = bootstrap_ioc.getContainer(c =>
				{
					c.For<IUserClarifySessionConfigurator>().Use(_userClarifySessionConfigurator);
					c.For<IClarifyApplicationFactory>().Use(_applicationFactory);
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
				return new ClarifyApplicationFactory(_settings).Create().CreateSession();
			}

			[SetUp]
			public void beforeEach()
			{
				_clarifyApplication = MockRepository.GenerateStub<IClarifyApplication>();
				_applicationFactory.Expect(s => s.Create()).Return(_clarifyApplication);
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
			public void should_use_application_username()
			{
				_applicationFactory.VerifyAllExpectations();
				_clarifyApplication.VerifyAllExpectations();
			}

			[Test]
			public void should_not_be_configured()
			{
				_userClarifySessionConfigurator.AssertWasCalled(a => a.Configure(_expectedSession));
			}
		}

		public class ejecting_a_session_user : session_cache_context
		{
			private ClarifySession _secondSession;
			private const string UserName = "annie";

			public override void setup()
			{
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_expectedSession).Repeat.Once();
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(true).Repeat.Once();
				
				_clarifyApplication.Stub(s => s.IsSessionValid(_expectedSession.SessionID)).Return(false).Repeat.Once();

				_secondSession = CreateRealSession();
				_clarifyApplication.Stub(s => s.CreateSession(UserName, ClarifyLoginType.User)).Return(_secondSession).Repeat.Once();
				_clarifyApplication.Stub(s => s.IsSessionValid(_secondSession.SessionID)).Return(true);
			}

			[Test]
			public void should_force_a_new_session_to_be_created()
			{
				_cut.GetSession(UserName).Id.ShouldEqual(_expectedSession.SessionID);
				_cut.EjectSession(UserName);
				_cut.GetSession(UserName).Id.ShouldEqual(_secondSession.SessionID); ;
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
				_applicationFactory.VerifyAllExpectations();
				_clarifyApplication.VerifyAllExpectations();
			}

			[Test]
			public void should_not_be_configured()
			{
				_userClarifySessionConfigurator.AssertWasNotCalled(a=>a.Configure(_expectedSession));
			}
		}
	}
}