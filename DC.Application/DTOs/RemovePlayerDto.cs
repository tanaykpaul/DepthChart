namespace DC.Application.DTOs
{
    public class RemovePlayerDto
    {
        public string PositionName { get; set; }
        public string PlayerName { get; set; }
        public string? TeamName { get; set; }
        public string? SportName { get; set; }
    }
}