using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Dovetail.SDK.Bootstrap.Authentication;
using Dovetail.SDK.Bootstrap.Clarify;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;
using FChoice.Foundation.Clarify.DataObjects;
using FChoice.Foundation.Clarify.Schema;
using FChoice.Foundation.DataObjects;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests.Clarify
{
	[TestFixture, Explicit]
	public class when_hundreds_of_sessions_are_inactive_and_someone_logs_in
	{
		private InMemoryClarifyApplication theApplication;
		private ClarifySessionCache theCache;
		private ObjectMother theData;
		private string[] theInactiveEmployees;
		private string[] theOtherEmployees;

		[SetUp]
		public void SetUp()
		{
			// TODO -- Pull this from the app config
			var clarify = ClarifyApplication.Initialize(new NameValueCollection
			{
				{ "fchoice.connectionstring", "Data Source=.; Initial Catalog=mobilecl125; User Id=sa; Password=sa;" },
				{"fchoice.dbtype", "mssql"},
				{"fchoice.disableloginfromfcapp", "false"},
				{"fchoice.sessionpasswordrequired", "false"},
				{"fchoice.nocachefile", "true"}
			});

			IClarifySessionUsageReporter reporter;

			theApplication = new InMemoryClarifyApplication(clarify);
			var logger = new NulloLogger();
			Func<IUserSessionStartObserver> startObserver = () =>
			{
				reporter = new ClarifySessionUsageReporter(theCache, theApplication, logger);
				return new SessionStartObserver(reporter);
			};

			theCache = new ClarifySessionCache(theApplication, logger,
				new NulloSessionConfigurator(),
				() => new SessionEndObserver(),
				startObserver,
				new DovetailDatabaseSettings());

			theData = new ObjectMother(clarify);

			theInactiveEmployees = theData.GenerateEmployees(950);
			theInactiveEmployees
				.Select(_ => theCache.GetSession(_))
				.ToList()
				.Each(_ => theApplication.InvalidateSession(_.Id));

			theOtherEmployees = theData.GenerateEmployees(5);

			Debug.WriteLine("Scenario created");
		}

		[TearDown]
		public void Teardown()
		{
			theData.CleanUp();
			Debug.WriteLine("Scenario cleaned up");
		}

		[Test]
		public void can_run_all_actions_in_parallel()
		{
			Parallel.Invoke(
				() => theCache.GetSession("sa"),
				() => theCache.GetSession(theInactiveEmployees[0]),
				() => theCache.GetSession(theInactiveEmployees[0]),
				() => theCache.GetSession(theInactiveEmployees[1]),
				() => theCache.CleanUpInvalidSessions(),
				() => theCache.GetSession(theOtherEmployees[0]),
				() => theCache.GetSession(theOtherEmployees[1]),
				() => theCache.GetSession(theOtherEmployees[2]),
				() => theCache.GetSession(theOtherEmployees[3]),
				() => theCache.GetSession(theOtherEmployees[4])
			);

			var sessions = theCache.SessionsByUsername.Values;
			var invalidSessions = sessions.Where(_ => !theApplication.IsSessionValid(_.Id)).ToArray();

			invalidSessions.Length.ShouldEqual(0);
		}

		private class NulloSessionConfigurator : IUserClarifySessionConfigurator
		{
			public void Configure(ClarifySession session)
			{
				// no-op
			}
		}

		private class SessionStartObserver : IUserSessionStartObserver
		{
			private readonly IClarifySessionUsageReporter _reporter;

			public SessionStartObserver(IClarifySessionUsageReporter reporter)
			{
				_reporter = reporter;
			}

			public void SessionStarted(IClarifySession session)
			{
				_reporter.GetActiveSessionCount();
			}
		}

		private class SessionEndObserver : IUserSessionEndObserver
		{
			public void SessionExpired(IClarifySession session)
			{
				// no-op
			}
		}

		private class InMemoryClarifyApplication : IClarifyApplication
		{
			private readonly ClarifyApplication _inner;

			private readonly List<Guid> _invalidIds = new List<Guid>();

			public InMemoryClarifyApplication(ClarifyApplication inner)
			{
				_inner = inner;
			}

			public void InvalidateSession(Guid id)
			{
				_invalidIds.Add(id);
			}

			public ClarifySession CreateSession()
			{
				throw new NotImplementedException();
			}

			public ClarifySession CreateSession(string userName, ClarifyLoginType loginType)
			{
				return _inner.CreateSession(userName, loginType);
			}

			public ClarifySession CreateSession(string userName, string password, ClarifyLoginType loginType)
			{
				throw new NotImplementedException();
			}

			public ClarifySession GetSession(Guid sessionID)
			{
				throw new NotImplementedException();
			}

			public bool IsSessionValid(Guid sessionID)
			{
				return !_invalidIds.Contains(sessionID);
			}

			public string GetMtmTableName(SchemaRelation relation)
			{
				throw new NotImplementedException();
			}

			public string EncryptDBPassword(string password)
			{
				throw new NotImplementedException();
			}

			public string EncryptClarifyPassword(string password)
			{
				throw new NotImplementedException();
			}

			public int GetObjID(string tableName)
			{
				throw new NotImplementedException();
			}

			public FCSessionSummary[] CurrentSessions()
			{
				throw new NotImplementedException();
			}

			public bool DataRestrictionsEnabled { get; set; }
			public NameValueCollection Configuration { get; set; }
			public string DBUserName { get; set; }
			public ITimeZone ServerTimeZone { get; set; }
			public bool ConvertTimeZone { get; set; }
			public bool TruncateStringFields { get; set; }
			public string ClarifyVersion { get; set; }
			public bool IsTravelerEnabled { get; set; }
			public bool IsDbUnicode { get; set; }
			public SchemaCache SchemaCache { get; set; }
			public ConfigItemCache ConfigItemCache { get; set; }
			public ClarifyConfigItemCollection ConfigItems { get; set; }
			public IListCache ListCache { get; set; }
			public ILocaleCache LocaleCache { get; set; }
			public IStringCache StringCache { get; set; }
			public Version DatabaseVersion { get; set; }
		}
	}
}
