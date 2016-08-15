using System;
using log4net;

namespace Dovetail.SDK.Clarify
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _log;

        public Log4NetLogger(Type type)
        {
            _log = LogManager.GetLogger(type.Assembly, type);
        }

        public void LogDebug(string message, params object[] parameters)
        {
            _log.DebugFormat(message, parameters);
        }

        public void LogDebug(string message)
        {
            _log.Debug(message);
        }

        public void LogInfo(string message, params object[] parameters)
        {
            _log.InfoFormat(message, parameters);
        }

        public void LogInfo(string message)
        {
            _log.Info(message);
        }

        public void LogWarn(string message, params object[] parameters)
        {
            _log.WarnFormat(message, parameters);
        }

        public void LogWarn(string message)
        {
            _log.Warn(message);
        }

        public void LogError(string message, params object[] parameters)
        {
            _log.ErrorFormat(message, parameters);
        }

        public void LogFatal(string message, Exception exception)
        {
            _log.Fatal(message, exception);
        }

        public void LogFatal(string message, params object[] parameters)
        {
            _log.FatalFormat(message, parameters);
        }

        public void LogError(string message, Exception exception)
        {
            _log.Error(message, exception);
        }
    }
}