namespace DC.Domain.Entities
{
    public class Position
    {
        public int PositionId { get; set; }
        public required string Name { get; set; }

        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public List<Order> Orders { get; } = [];
        public List<Team> Teams { get; } = [];
    }
}