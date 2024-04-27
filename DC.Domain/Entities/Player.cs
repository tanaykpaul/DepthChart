namespace DC.Domain.Entities
{
    public class Player
    {
        public int PlayerId { get; set; }
        public required int Number { get; set; }
        public required string Name { get; set; }
        public string? Odds { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public List<Order> Orders { get; } = [];
        public List<Position> Positions { get; } = [];
    }
}