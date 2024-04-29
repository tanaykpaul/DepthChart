namespace DC.Application.DTOs
{
    public class PlayerDTO
    {
        public required int Number { get; set; }
        public required string Name { get; set; }
        public string? Odds { get; set; }
        public int TeamId { get; set; }
    }
}