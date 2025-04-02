using System;

namespace THEBADDEST.SimpleDependencyInjection
{
    /// <summary>
    /// Log level for dependency injection operations
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Interface for dependency injection logging
    /// </summary>
    public interface ILogger
    {
        void Log(LogLevel level, string message, Exception exception = null);
        bool IsEnabled(LogLevel level);
    }

    /// <summary>
    /// Default implementation of ILogger
    /// </summary>
    public class DefaultLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;

        public DefaultLogger(LogLevel minimumLevel = LogLevel.Info)
        {
            _minimumLevel = minimumLevel;
        }

        public void Log(LogLevel level, string message, Exception exception = null)
        {
            if (!IsEnabled(level))
                return;

            var logMessage = $"[SimpleDI] [{level}] {message}";
            if (exception != null)
            {
                logMessage += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
            }

            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(logMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(logMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(logMessage);
                    break;
            }
        }

        public bool IsEnabled(LogLevel level)
        {
            return level >= _minimumLevel;
        }
    }

    /// <summary>
    /// Null implementation of ILogger that does nothing
    /// </summary>
    public class NullLogger : ILogger
    {
        public void Log(LogLevel level, string message, Exception exception = null)
        {
        }

        public bool IsEnabled(LogLevel level)
        {
            return false;
        }
    }
}