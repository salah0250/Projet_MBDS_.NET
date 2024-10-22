using AutoMapper;
using Gauniv.WebServer.Data;

namespace Gauniv.WebServer.Dtos
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Rajouter autant de ligne ici que vous avez de mapping Model <-> DTO
            // https://docs.automapper.org/en/latest/
            CreateMap<Game, GameDto>();
        }
    }
}
