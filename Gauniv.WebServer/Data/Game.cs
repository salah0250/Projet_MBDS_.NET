using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Gauniv.WebServer.Data
{
    public class Game
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime ReleaseDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Assurez-vous d'initialiser CreatedAt avec UTC.

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]  // Spécifie que la catégorie sera stockée en tant que chaîne de caractères
        public Category Category { get; set; } // Enumération Category

        public byte[] Payload { get; set; } // Le binaire du jeu
        public byte[] Image { get; set; }
    }

}
