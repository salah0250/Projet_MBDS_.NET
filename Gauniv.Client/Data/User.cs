using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Gauniv.Client.Data
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
