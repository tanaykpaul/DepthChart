using DC.Application.DTOs;
using DC.Domain.Logging;
using System.Text.Json;

namespace DC.Application.Services
{
    public class ConsumeInputFromJson : IConsumeInput<DepthChartDTO>
    {
        private readonly IAppLogger _logger;

        public ConsumeInputFromJson(IAppLogger logger)
        {
            _logger = logger;
        }

        public DepthChartDTO? GetData(string fileContents)
        {
            try
            {
                if(!string.IsNullOrWhiteSpace(fileContents))
                {
                    _logger.LogInformation("Performing Json Serializtion...");
                    return JsonSerializer.Deserialize<DepthChartDTO>(fileContents);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Input JSON file is incorrect. The exception messgage is: {ex.Message}");
                return null;
            }
        }
    }
}