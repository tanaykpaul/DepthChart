namespace DC.Domain.Entities
{
    public class Sport
    {
        public int SportId { get; set; }
        public required string Name { get; set; }

        public ICollection<Team> Teams { get; } = [];
    }
}