using DC.Domain.Logging;
using Microsoft.Extensions.Logging;

namespace DC.Infrastructure.Logging
{
    public class AppLogger : IAppLogger
    {
        private readonly ILogger<AppLogger> _logger;

        public AppLogger(ILogger<AppLogger> logger)
        {
            _logger = logger;
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