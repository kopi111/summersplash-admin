using System.Collections.Concurrent;

namespace SummerSplashWeb.Services
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logsPath;
        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new();

        public FileLoggerProvider(string logsPath)
        {
            _logsPath = logsPath;

            // Ensure logs directory exists
            if (!Directory.Exists(_logsPath))
            {
                Directory.CreateDirectory(_logsPath);
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _logsPath));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class FileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logsPath;
        private static readonly object _lock = new();

        public FileLogger(string categoryName, string logsPath)
        {
            _categoryName = categoryName;
            _logsPath = logsPath;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logFileName = Path.Combine(_logsPath, $"app-{DateTime.Now:yyyy-MM-dd}.log");
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {formatter(state, exception)}";

            if (exception != null)
            {
                logEntry += Environment.NewLine + exception.ToString();
            }

            lock (_lock)
            {
                try
                {
                    File.AppendAllText(logFileName, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Silently fail if we can't write to log file
                }
            }
        }
    }

    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(this ILoggingBuilder builder, string logsPath)
        {
            builder.AddProvider(new FileLoggerProvider(logsPath));
            return builder;
        }
    }
}
