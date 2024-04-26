namespace DC.Application.DTOs
{
    public class AddPlayerDto
    {
        public string PositionName { get; set; }
        public string PlayerName { get; set; }
        public int? OrderNumber { get; set; }
        public string? TeamName { get; set; }
        public string? SportName { get; set; }
    }
}