using System;
using System.Diagnostics;

namespace Dovetail.SDK.Bootstrap.Tests.Clarify
{
	public class NulloLogger : ILogger, IDisposable
	{
		public void LogDebug(string message, params object[] parameters)
		{
			//Debug.WriteLine(message, parameters);
		}

		public void LogDebug(string message)
		{
			//Debug.WriteLine(message);
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
