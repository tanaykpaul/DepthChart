using DC.Application.DTOs;
using System.Text.Json;

namespace DC.Application.Services
{
    public class ConsumeInputFromJson : IConsumeInput<DepthChartDto>
    {
        public DepthChartDto? GetData(string fileContents)
        {
            try
            {
                if(!string.IsNullOrWhiteSpace(fileContents))
                {
                    return JsonSerializer.Deserialize<DepthChartDto>(fileContents);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Input JSON file is incorrect. The exception messgage is: {ex.Message}");
            }
        }
    }
}