using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
	public class session_usage_reporter : Context<ClarifySessionUsageReporter>
	{
		private Dictionary<string, IClarifySession> _sessionsByUser;
		private IClarifyApplication _app;
		private IClarifySessionUsage _result;

		public override void Given()
		{
			_sessionsByUser = new Dictionary<string, IClarifySession>
				{
					{"annie", getMockSession()}, 
					{"hank", getMockSession()}
				};
			MockFor<IClarifySessionCache>().Stub(s => s.SessionsByUsername).Return(_sessionsByUser);

			_app = MockFor<IClarifyApplication>();
			MockFor<IClarifyApplicationFactory>().Stub(s => s.Create()).Return(_app);

			_app.Stub(s => s.IsSessionValid(_sessionsByUser["hank"].Id)).Return(true);
			_app.Stub(s => s.IsSessionValid(_sessionsByUser["annie"].Id)).Return(false);

			_result = _cut.GetUsage();
		}

		[Test]
		public void should_group_sessions_correctly()
		{
			_result.Sessions.Count().ShouldEqual(1);
			_result.InvalidSessions.Count().ShouldEqual(1);
		}

		[Test]
		public void should_eject_invalid_sessions()
		{
			MockFor<IClarifySessionCache>().AssertWasCalled(a=>a.EjectSession("annie"));
		}

		[Test]
		public void should_not_eject_valid_sessions()
		{
			MockFor<IClarifySessionCache>().AssertWasNotCalled(a => a.EjectSession("hank"));
		}


		private IClarifySession getMockSession()
		{
			var session = _services.AddAdditionalMockFor<IClarifySession>();
			session.Stub(s => s.Id).Return(Guid.NewGuid());
			return session;
		}
	}
}