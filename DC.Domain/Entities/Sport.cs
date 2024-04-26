using System.ComponentModel.DataAnnotations;

namespace DC.Domain.Entities
{
    public class Sport
    {
        [Key]
        public int SportId { get; set; }
        [Required]
        public string Name { get; set; }

        // One-to-many relationships
        public ICollection<Team> Teams { get; set; }
    }
}