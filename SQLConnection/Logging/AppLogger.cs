using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SQLConnection.Logging
{
    [Obsolete("Use Microsoft.Extensions.Logging. AppLogger remains for backward compatibility.")]
    public static class AppLogger
    {
        private static readonly object _lock = new object();
        private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");

        public static void LogInformation(string message)
        {
            Log("INFO", message);
        }

        public static void LogWarning(string message)
        {
            Log("WARN", message);
        }

        public static void LogError(string message)
        {
            Log("ERROR", message);
        }

        public static void LogError(string message, Exception ex)
        {
            Log("ERROR", message + " - " + ex);
        }

        private static void Log(string level, string message)
        {
            try
            {
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
                lock (_lock)
                {
                    File.AppendAllText(_logPath, line, Encoding.UTF8);
                }
            }
            catch
            {
                // Swallow logging exceptions - logging should not break app
            }
        }

        // Adapter helper to create an ILogger<T> from a logger factory using simple Console logging if needed.
        public static ILogger<T> CreateLoggerAdapter<T>()
        {
            using var factory = LoggerFactory.Create(builder => builder.AddConsole());
            return factory.CreateLogger<T>();
        }
    }
}
