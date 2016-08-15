using System;

namespace Dovetail.SDK.Clarify
{
    public interface ILogger
    {
        void LogDebug(string message, params object[] parameters);
        void LogDebug(string message);
        void LogInfo(string message, params object[] parameters);
        void LogInfo(string message);
        void LogWarn(string message, params object[] parameters);
        void LogWarn(string message);
        void LogError(string message, Exception exception);
        void LogError(string message, params object[] parameters);
        void LogFatal(string message, Exception exception);
        void LogFatal(string message, params object[] parameters);
    }
}