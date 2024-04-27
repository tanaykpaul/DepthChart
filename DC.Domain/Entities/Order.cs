namespace DC.Domain.Entities
{
    public class Order
    {
        public required int SeqNumber { get; set; }

        public int PositionId { get; set; }
        public int PlayerId { get; set; }
        public Position Position { get; set; } = null!;
        public Player Player { get; set; } = null!;
    }
}