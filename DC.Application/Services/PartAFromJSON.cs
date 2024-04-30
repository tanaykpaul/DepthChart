using AutoMapper;
using DC.Application.DTOs;
using DC.Domain.Entities;
using DC.Domain.Logging;
using System.Text.Json;

namespace DC.Application.Services
{
    public class PartAFromJSON : IProcessInput<Sport?, SportDTO>
    {
        private readonly IAppLogger _logger;
        private readonly IMapper _mapper;

        public PartAFromJSON(IAppLogger logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the JSON string content to the Sport object
        /// </summary>
        /// <param name="fileContents">JSON string contents</param>
        /// <returns>Sport object</returns>
        public Sport? GetData(string fileContents)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(fileContents))
                {
                    _logger.LogInformation("Performing Json Serializtion...");
                    var sportDTO = JsonSerializer.Deserialize<SportDTO>(fileContents);
                    var sport = _mapper.Map<Sport>(sportDTO);

                    return sport;
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