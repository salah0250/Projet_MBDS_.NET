using System;
using System.Collections.Generic;

namespace Gauniv.Client.DTOs
{
    public class UserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public List<UserGameDto> UserGames { get; set; } = new();
    }

    public class UserGameDto
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; }
        public DateTime PurchaseDate { get; set; }
    }
}

