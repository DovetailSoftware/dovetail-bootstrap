using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using NUnit.Framework;
using Rhino.Mocks;

namespace Dovetail.SDK.Bootstrap.Tests
{
	public abstract class session_usage_reporter_context : Context<ClarifySessionUsageReporter>
	{
		protected Dictionary<string, IClarifySession> _sessionsByUser;
		protected IClarifyApplication _app;
		protected IClarifySessionUsage _result;

		public override void Given()
		{
			_sessionsByUser = new Dictionary<string, IClarifySession>();
			_app = MockFor<IClarifyApplication>();

			addMockSessionFor("annie", false);
			addMockSessionFor("hank", true);
			addMockSessionFor("sven", true);

			MockFor<IClarifySessionCache>().Stub(s => s.SessionsByUsername).Return(_sessionsByUser);
		}

		private void addMockSessionFor(string username, bool isValid)
		{
			var id = Guid.NewGuid();

			var session = _services.AddAdditionalMockFor<IClarifySession>();
			session.Stub(s => s.Id).Return(id);
			session.Stub(s => s.UserName).Return(username);

			_app.Stub(s => s.IsSessionValid(id)).Return(isValid);

			_sessionsByUser.Add(username, session);
		}
	}

	public class session_usage_reporter : session_usage_reporter_context
	{
		[Test]
		public void should_group_sessions_correctly()
		{
			_result = _cut.GetUsage();

			_result.Sessions.Count().ShouldEqual(2);
			_result.InvalidSessions.Count().ShouldEqual(1);
		}
	}
}
