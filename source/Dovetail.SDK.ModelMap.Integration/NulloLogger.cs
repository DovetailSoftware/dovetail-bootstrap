using System;
using Dovetail.SDK.Bootstrap;

namespace Dovetail.SDK.ModelMap.Integration
{
	public class NulloLogger : ILogger, IDisposable
	{
		public void LogDebug(string message, params object[] parameters)
		{
		}

		public void LogDebug(string message)
		{
		}

		public void LogInfo(string message, params object[] parameters)
		{
		}

		public void LogInfo(string message)
		{
		}

		public void LogWarn(string message, params object[] parameters)
		{
		}

		public void LogWarn(string message)
		{
		}

		public void LogError(string message, Exception exception)
		{
		}

		public void LogError(string message, params object[] parameters)
		{
		}

		public void LogFatal(string message, Exception exception)
		{
		}

		public void LogFatal(string message, params object[] parameters)
		{
		}

		public IDisposable Push(string context)
		{
			return this;
		}

		public void Dispose()
		{
		}
	}
}