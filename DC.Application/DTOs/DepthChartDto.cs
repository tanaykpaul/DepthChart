namespace DC.Application.DTOs
{
    public class DepthChartDto
    {
        public string Sport { get; set; }
        public TeamDto[] Teams { get; set; }
    }

    public class TeamDto
    {
        public string Name { get; set; }
        public PositionDto[] Positions { get; set; }
    }

    public class PositionDto
    {
        public string Name { get; set; }
        public OrderDto[] Orders { get; set; }
    }

    public class OrderDto
    {
        public int SeqNumber { get; set; }
        public PayerDetailsDto PlayerDetails { get; set; }
    }

    public class PayerDetailsDto
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public string Odds { get; set; }
    }
}