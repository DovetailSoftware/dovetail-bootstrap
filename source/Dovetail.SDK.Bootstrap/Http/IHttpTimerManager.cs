using System;
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
		private readonly ILogger _logger;

		public HttpTimerManager(IServiceLocator services, IHttpApplicationStorage storage, ILogger logger)
		{
			_services = services;
			_storage = storage;
			_logger = logger;
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
				try
				{
					_services
						.GetInstance<TTService>()
						.Execute();
				}
				catch (Exception e)
				{
					_logger.LogError("Error executing " + typeof(TTService).Name, e);
				}

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
