namespace DC.Application.DTOs
{
    public class OrderDTO
    {
        public required int SeqNumber { get; set; }
        public required string PositionName { get; set; }
        public int PlayerNumber { get; set; }
    }
}