namespace Gauniv.WebServer.Dtos
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public ICollection<UserGameDto> Games { get; set; }
    }
}
