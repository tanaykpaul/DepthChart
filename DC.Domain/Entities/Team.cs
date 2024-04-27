namespace DC.Domain.Entities
{
    public class Team
    {
        public int TeamId { get; set; }
        public required string Name { get; set; }

        public int SportId { get; set; }
        public Sport Sport { get; set; } = null!;

        public ICollection<Position> Positions { get; } = [];
        public ICollection<Player> Players { get; } = [];
    }
}