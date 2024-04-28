namespace DC.Application.DTOs
{
    public class PlayerCreationResponseDTO
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int PlayerNumber { get; set; }
        public string Odds { get; set; }    
        public int TeamId { get; set; }
    }
}