using AutoMapper;
using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Logging;
using System.Text.Json;

namespace DC.Application.Services
{
    /// <summary>
    /// STP2 stands for Sport -> Teams -> Positions and Players
    /// </summary>
    public class STP2FromJSON : IProcessInput<Sport?, SportDTO>
    {
        private readonly IAppLogger _logger;
        private readonly IMapper _mapper;

        public STP2FromJSON(IAppLogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the JSON string content to the Sport object
        /// </summary>
        /// <param name="fileContents">JSON string contents</param>
        /// <returns>Sport object</returns>
        public (Sport?, SportDTO?) GetData(string fileContents)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(fileContents))
                {
                    _logger.LogInformation("Performing Json Serializtion...");
                    var sportDTO = JsonSerializer.Deserialize<SportDTO>(fileContents);
                    var sport = _mapper.Map<Sport>(sportDTO);

                    return (sport, sportDTO);
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Input JSON file is incorrect. The exception messgage is: {ex.Message}");
                return (null, null);
            }
        }
    }
}