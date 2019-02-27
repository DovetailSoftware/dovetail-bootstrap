using System;
using System.Threading;
using System.Web.SessionState;
using Dovetail.SDK.Bootstrap.Http;
using Dovetail.SDK.Bootstrap.Tests.Clarify;
using FubuCore;
using NUnit.Framework;

namespace Dovetail.SDK.Bootstrap.Tests.Http
{
	[TestFixture]
	public class HttpTimerManagerTester
	{
		[Test]
		public void resolves_the_key_of_the_service()
		{
			HttpTimerManager.ResolveKey<LambdaHttpService>().ShouldEqual(typeof(LambdaHttpService).FullName);
		}

		[Test]
		public void starts_stops_and_stores_the_timer()
		{
			var autoEvent = new AutoResetEvent(false);
			var clean = false;
			var service = new LambdaHttpService(() => { autoEvent.Set(); }, () => { clean = true; })
			{
				Interval = 100
			};

			var services = new InMemoryServiceLocator();
			services.Add(service);

			var storage = new ThreadHttpApplicationStorage();
			var manager = new HttpTimerManager(services, storage, new NulloLogger());

			manager.Start<LambdaHttpService>();
			autoEvent.WaitOne(1000);

			var key = HttpTimerManager.ResolveKey<LambdaHttpService>();
			storage.Has(key).ShouldBeTrue();

			manager.Stop<LambdaHttpService>();

			storage.Has(key).ShouldBeFalse();

			clean.ShouldBeTrue();
		}

		private class LambdaHttpService : IHttpIntervalService
		{
			private readonly Action _execute;
			private readonly Action _cleanup;

			public LambdaHttpService(Action execute, Action cleanup)
			{
				_execute = execute;
				_cleanup = cleanup;
			}

			public int Interval { get; set; }

			public void Execute()
			{
				_execute();
			}

			public void CleanUp()
			{
				_cleanup();
			}
		}
	}
}
