using AutoMapper;
using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;

namespace Gauniv.WebServer.Mappings
{
    public class GameProfile : Profile
    {
        public GameProfile()
        {
            // Game -> GameDto mapping
            CreateMap<Game, GameDto>()
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src =>
                    src.UserGames != null ?
                    src.UserGames.Select(ug => new GameUserDto
                    {
                        UserId = ug.UserId,
                        UserName = ug.User != null ? ug.User.UserName : string.Empty,
                        PurchaseDate = ug.PurchaseDate
                    }) : new List<GameUserDto>()))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.ReleaseDate))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image));

            // GameDto -> Game mapping
            CreateMap<GameDto, Game>()
                .ForMember(dest => dest.UserGames, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ReleaseDate, opt => opt.MapFrom(src => src.ReleaseDate))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image));

            // User -> UserDto mapping
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Games, opt => opt.MapFrom(src =>
                    src.UserGames != null ?
                    src.UserGames.Select(ug => new UserGameDto
                    {
                        GameId = ug.GameId,
                        Title = ug.Game != null ? ug.Game.Title : string.Empty,
                        PurchaseDate = ug.PurchaseDate
                    }) : new List<UserGameDto>()));

            // UserDto -> User mapping
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.UserGames, opt => opt.Ignore());
        }
    }
}