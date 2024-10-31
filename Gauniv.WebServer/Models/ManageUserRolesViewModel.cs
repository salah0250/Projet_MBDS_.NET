using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Gauniv.WebServer.Models
{
    public class ManageUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

        // Liste des rôles disponibles
        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();

        // Liste des rôles assignés à l'utilisateur
        public List<string> AssignedRoles { get; set; } = new List<string>();
    }
}
