using System;
using System.Collections.Generic;
using System.Linq;
using Dovetail.SDK.Bootstrap.Clarify;
using Dovetail.SDK.Bootstrap.Configuration;
using FChoice.Common.Licensing;
using NUnit.Framework;
using StructureMap;

namespace Dovetail.SDK.ModelMap.Integration.Session
{
	[TestFixture, Explicit]
	public class session_load 
	{
		protected IContainer _container;
		protected ClarifySessionCache _cut;

		[TestFixtureSetUp]
		public void beforeAll()
		{
			_container = bootstrap_ioc.getContainer(c => c.AddRegistry<BootstrapRegistry>());

			_cut = _container.GetInstance<ClarifySessionCache>();
		}

		[TestFixtureTearDown]
		public void afterAll()
		{
		}

		public class LicLog
		{
			public IClarifySession Session { get; set; }
			public int Remaining { get; set; }
			public int GraceRemaining { get; set; }
			public int GraceEventsRemaining { get; set; }

			public override string ToString()
			{
				return String.Format("Left: {0}, GraceLeft: {1}", Remaining, GraceRemaining);
			}
		}

		[Test]
		public void reserve_lots_of_licenses()
		{
			var mother = new ObjectMother(_cut.GetApplicationSession());

			logSDKLicense();

			var licLog = Enumerable.Range(0, 14).Select(index =>
				{
					var session = _cut.GetSession(mother.CreateEmployee().Login);
					var lic = getSDKLicense();

					return new LicLog
						{
							Remaining = lic.UserLicensesRemaining,
							GraceRemaining = lic.GraceLicensesRemaining,
							GraceEventsRemaining = lic.GraceEventsRemaining,
							Session = session
						};
				}).ToArray();

			licLog.Each(r=>Console.WriteLine(r.ToString()));


			foreach (var log in licLog)
			{
				_cut.EjectSession(log.Session.UserName);
				logSDKLicense();
			}
		}

		private static void logSDKLicense()
		{
			var sdk = getSDKLicense();
			Console.WriteLine("SDK license type {0}\nuser limit:{1}\nexpires {3}\n{4} user license remaining\n{2} grace events remaining.\n", 
				sdk.UserRestriction, 
				sdk.UserLicenseLimit, 
				sdk.GraceEventsRemaining, 
				sdk.ExpirationDate.ToShortDateString(), 
				sdk.UserLicensesRemaining);
		}

		private static ILicenseInfo getSDKLicense()
		{
			return LicenseManager.Instance.GetLicenseInformation(1);
		}
	}
}