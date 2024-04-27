namespace DC.Application.DTOs
{
    public class AddPlayerToDepthChartDTO
    {
        public required string PositionName { get; set; }
        public int PlayerNumber { get; set; }
        public int? DepthPosition { get; set; }
    }
}