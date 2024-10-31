using System.ComponentModel.DataAnnotations;

namespace Gauniv.WebServer.Data
{
    public class UserGame
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // ID de l'utilisateur

        [Required]
        public int GameId { get; set; } // ID du jeu

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        // Relations
        public virtual User User { get; set; }
        public virtual Game Game { get; set; }
    }
}
