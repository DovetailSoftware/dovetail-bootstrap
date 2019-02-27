using System.Threading;
using FubuCore;

namespace Dovetail.SDK.Bootstrap.Http
{
	public interface IHttpTimerManager
	{
		void Start<TTService>() where TTService : IHttpIntervalService;
		void Stop<TTService>() where TTService : IHttpIntervalService;
	}

	public class HttpTimerManager : IHttpTimerManager
	{
		private readonly IServiceLocator _services;
		private readonly IHttpApplicationStorage _storage;

		public HttpTimerManager(IServiceLocator services, IHttpApplicationStorage storage)
		{
			_services = services;
			_storage = storage;
		}

		public void Start<TTService>() where TTService : IHttpIntervalService
		{
			var key = ResolveKey<TTService>();
			if (_storage.Has(key))
			{
				Stop<TTService>();
			}

			var service = _services.GetInstance<TTService>();
			var timer = new Timer(state =>
			{
				_services
					.GetInstance<TTService>()
					.Execute();
			});

			timer.Change(service.Interval, service.Interval);
			_storage.Store(key, timer);
		}

		public void Stop<TTService>() where TTService : IHttpIntervalService
		{
			var key = ResolveKey<TTService>();
			if (!_storage.Has(key))
				return;

			_storage
				.Get(key)
				.As<Timer>()
				.Dispose();

			_services
				.GetInstance<TTService>()
				.CleanUp();

			_storage.Remove(key);
		}

		public static string ResolveKey<TTimer>()
		{
			return typeof(TTimer).FullName;
		}
	}
}
