namespace DC.Application.DTOs
{
    public class TeamDTO
    {
        public required string Name { get; set; }
        public int SportId { get; set; }
        public PositionDTO[]? Positions { get; set; }
        public PlayerDTO[]? Players { get; set; }
        public OrderDTO[]? Orders { get; set; }
    }
}