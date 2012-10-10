using System;
using System.Web;
using StructureMap;

namespace Dovetail.SDK.Bootstrap.Configuration
{
	public class LogRequestContextModule : IHttpModule
	{
		private ILogger _logger;
		private IDisposable _logContext;

		public void Init(HttpApplication context)
		{
			context.BeginRequest += (sender, args) =>
			{
				_logger = ObjectFactory.GetInstance<ILogger>();

				var url = HttpContext.Current.Request.Path;

				_logContext = _logger.Push(url);
			};

			context.EndRequest += (sender, args) => Dispose();
		}

		public void Dispose()
		{
			if (_logContext == null) return;

			_logContext.Dispose();
			_logContext = null;
		}
	}
}