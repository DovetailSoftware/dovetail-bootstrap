using System;

namespace Dovetail.SDK.Clarify
{
    public class NulloLogger : ILogger
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
    }
}