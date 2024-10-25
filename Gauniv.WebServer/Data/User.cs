using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.WebServer.Data
{
    public class User : IdentityUser
    {
        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        public virtual ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();

    }
}
