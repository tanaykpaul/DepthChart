using AutoMapper;
using DC.Application.DTOs;
using DC.Domain.Entities;

namespace DC.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Configure mappings between entities and DTOs
            //CreateMap<Sport, SportDTO>();
            CreateMap<SportDTO, Sport>()
            .ForMember(dest => dest.SportId, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Teams, opt => opt.MapFrom(src => src.Teams));

            //CreateMap<Team, TeamDTO>();
            CreateMap<TeamDTO, Team>()
            .ForMember(dest => dest.TeamId, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.SportId, opt => opt.MapFrom(src => src.SportId))
            .ForMember(dest => dest.Players, opt => opt.MapFrom(src => src.Players))
            .ForMember(dest => dest.Positions, opt => opt.MapFrom(src => src.Positions))
            .ForMember(dest => dest.Sport, opt => opt.Ignore());

            //CreateMap<Position, PositionDTO>();
            CreateMap<PositionDTO, Position>()
            .ForMember(dest => dest.PositionId, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId))
            .ForMember(dest => dest.Team, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore())
            .ForMember(dest => dest.Teams, opt => opt.Ignore());

            //CreateMap<Player, PlayerDTO>();
            CreateMap<PlayerDTO, Player>()
            .ForMember(dest => dest.PlayerId, opt => opt.Ignore())
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Odds, opt => opt.MapFrom(src => src.Odds))
            .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.TeamId))
            .ForMember(dest => dest.Team, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

            //CreateMap<Order, OrderDTO>();
            CreateMap<OrderDTO, Order>()
            .ForMember(dest => dest.SeqNumber, opt => opt.MapFrom(src => src.SeqNumber))
            .ForMember(dest => dest.PositionId, opt => opt.MapFrom(src => src.PositionName))
            .ForMember(dest => dest.PlayerId, opt => opt.MapFrom(src => src.PlayerNumber))
            .ForMember(dest => dest.Position, opt => opt.Ignore())
            .ForMember(dest => dest.Player, opt => opt.Ignore());
        }
    }
}