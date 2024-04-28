namespace DC.Application.DTOs
{
    public class RemovePlayerFromDepthChartDTO
    {
        public required string PositionName { get; set; }
        public int PlayerNumber { get; set; }
    }
}