using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Gauniv.WebServer.Models
{
    public class CreateViewModel()
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public float Price { get; set; }

        [Required]
        public IFormFile Content { get; set; }

        public int[] Categories { get; set; }
    }
    public class EditViewModel()
    {
        
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Name { get; set; }

        [Range(0.01, double.MaxValue)]
        public float? Price { get; set; }

        [Required]
        public IFormFile? Content { get; set; }
        public int[]? Categories { get; set; }
    }
}
