using System.ComponentModel.DataAnnotations;

namespace DC.Domain.Entities
{
    public class Position
    {
        [Required]
        public int PositionId { get; set; }
        [Required]
        public string Name { get; set; }

        // Foreign key
        public int TeamId { get; set; }
        public Team Team { get; set; }

        // One-to-many relationship
        public List<Order> Orders { get; set; }
    }
}