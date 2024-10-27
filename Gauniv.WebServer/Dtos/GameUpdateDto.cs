using Gauniv.WebServer.Data;

namespace Gauniv.WebServer.Dtos
{
    public class GameUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Category Category { get; set; }
        public DateTime ReleaseDate { get; set; }
        public IFormFile? ImageFile { get; set; }
        public IFormFile? PayloadFile { get; set; }
    }
}
