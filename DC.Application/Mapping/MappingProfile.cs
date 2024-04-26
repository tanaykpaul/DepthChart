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
            CreateMap<Sport, SportDTO>();
            CreateMap<SportDTO, Sport>();

            CreateMap<Team, TeamDTO>();
            CreateMap<TeamDTO, Team>();

            CreateMap<Position, PositionDTO>();
            CreateMap<PositionDTO, Position>();

            CreateMap<Player, PlayerDTO>();
            CreateMap<PlayerDTO, Player>();

            CreateMap<Order, OrderDTO>();
            CreateMap<OrderDTO, Order>();
        }
    }
}