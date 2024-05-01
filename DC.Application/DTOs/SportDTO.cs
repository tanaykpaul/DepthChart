namespace DC.Application.DTOs
{
    public class SportDTO
    {
        public required string Name { get; set; }
        public TeamDTO[]? Teams { get; set; }
    }
}