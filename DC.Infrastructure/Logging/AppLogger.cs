using DC.Domain.Logging;
using Microsoft.Extensions.Logging;

namespace DC.Infrastructure.Logging
{
    public class AppLogger<T> : IAppLogger<T>
    {
        private readonly ILogger<T> _logger;

        public AppLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message);
        }
    }
}