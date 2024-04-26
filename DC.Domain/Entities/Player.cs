using System.ComponentModel.DataAnnotations;

namespace DC.Domain.Entities
{
    public class Player
    {
        [Key]
        public int PlayerId { get; set; }
        [Required]
        public int Number { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Odds { get; set; }

        // Foreign key
        public int TeamId { get; set; }
        public Team Team { get; set; }

        // One-to-many relationship
        public List<Order> Orders { get; set; } = new List<Order>();
    }
}