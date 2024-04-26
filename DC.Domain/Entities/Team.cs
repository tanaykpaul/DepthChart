using System.ComponentModel.DataAnnotations;

namespace DC.Domain.Entities
{
    public class Team
    {
        [Key]
        public int TeamId { get; set; }
        [Required]
        public string Name { get; set; }

        // Foreign key
        public int SportId { get; set; }
        public Sport Sport { get; set; }

        // One-to-many relationships
        public List<Position> Positions { get; set; } = new List<Position>();
        public List<Player> Players { get; set; } = new List<Player>();
    }
}