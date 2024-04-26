using System.ComponentModel.DataAnnotations;

namespace DC.Domain.Entities
{
    public class Order
    {
        [Required]
        public int OrderId { get; set; }
        [Required]
        public int SeqNumber { get; set; }

        // Foreign keys
        public int PositionId { get; set; }
        public Position Position { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }
    }
}