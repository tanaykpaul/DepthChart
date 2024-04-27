namespace DC.Application.DTOs
{
    public class PlayerDTO
    {
        public int PlayerNumber { get; set; }
        public required string Name { get; set; }
        public string? Odds { get; set; }
        public int TeamId { get; set; }
    }
}