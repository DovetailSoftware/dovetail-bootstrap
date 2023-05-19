using System;
using System.Collections.Specialized;
using System.Linq;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation.Clarify;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests.Clarify
{
	[TestFixture, Explicit]
	public class SameUserLogsInMoreThanOnce
	{
		private InMemoryClarifyApplication _theApplication;
		private ClarifySessionCache _theCache;
		private ObjectMother _theData;

		[SetUp]
		public void SetUp()
		{
			// TODO -- Pull this from the app config
			var clarify = ClarifyApplication.Initialize(new NameValueCollection
			{
				{"fchoice.connectionstring", "Data Source=.; Initial Catalog=mobilecl125; User Id=sa; Password=sa;"},
				{"fchoice.dbtype", "mssql"},
				{"fchoice.disableloginfromfcapp", "false"},
				{"fchoice.sessionpasswordrequired", "false"},
				{"fchoice.nocachefile", "true"}
			});

			IClarifySessionUsageReporter reporter;

			_theApplication = new InMemoryClarifyApplication(clarify);
			var logger = new NulloLogger();
			Func<IUserSessionStartObserver> startObserver = () =>
			{
				reporter = new ClarifySessionUsageReporter(_theCache, _theApplication, logger);
				return new SessionStartObserver(reporter);
			};

			_theCache = new ClarifySessionCache(_theApplication, logger,
				new NulloSessionConfigurator(),
				() => new SessionEndObserver(),
				startObserver,
				new DovetailDatabaseSettings());

			_theData = new ObjectMother(clarify);
		}

		[TearDown]
		public void Teardown()
		{
			_theData.CleanUp();
		}

		[Test]
		public void always_have_one_session_per_user_with_lowercase_UserName()
		{
			var annieUCSession = _theCache.GetSession("ANNIE");
			annieUCSession.ShouldNotBeNull();

			var sessions = _theCache.SessionsByUsername.Values.ToArray();
			sessions.Length.ShouldEqual(1);

			sessions.FirstOrDefault(s => s.UserName == "annie").ShouldNotBeNull();

			var annieLCSession = _theCache.GetSession("annie");
			annieLCSession.ShouldNotBeNull();

			annieLCSession.ShouldBeTheSameAs(annieUCSession);
			sessions.Length.ShouldEqual(1);

			var annieMCSession = _theCache.GetSession("AnNiE");
			annieMCSession.ShouldNotBeNull();

			annieMCSession.ShouldBeTheSameAs(annieUCSession);
			sessions.Length.ShouldEqual(1);
		}
	}
}
