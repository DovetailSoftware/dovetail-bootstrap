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
					{"hank", getMockSession()},
					{"sven", getMockSession()}
				};
			MockFor<IClarifySessionCache>().Stub(s => s.SessionsByUsername).Return(_sessionsByUser);

			_app = MockFor<IClarifyApplication>();
			_app.Stub(s => s.IsSessionValid(_sessionsByUser["hank"].Id)).Return(true);
			_app.Stub(s => s.IsSessionValid(_sessionsByUser["annie"].Id)).Return(false);
			_app.Stub(s => s.IsSessionValid(_sessionsByUser["sven"].Id)).Return(true);
		}

		[Test]
		public void get_usage_should_group_sessions_correctly()
		{
			_result = _cut.GetUsage();

			_result.Sessions.Count().ShouldEqual(2);
			_result.InvalidSessions.Count().ShouldEqual(1);
		}

		[Test]
		public void get_active_session_count_should_return_count_of_valid_sessions()
		{
			_cut.GetActiveSessionCount().ShouldEqual(2);
		}
		
		private IClarifySession getMockSession()
		{
			var session = _services.AddAdditionalMockFor<IClarifySession>();
			session.Stub(s => s.Id).Return(Guid.NewGuid());
			return session;
		}
	}
}