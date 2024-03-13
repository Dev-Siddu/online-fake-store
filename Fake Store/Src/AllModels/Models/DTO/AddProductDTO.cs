using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class AddProductDTO
    {
        [Required(ErrorMessage = "Enter Name")]
        public string? Name { get; set; }
        public string? Description { get; set; } = null;

        [Required(ErrorMessage = "Enter price")]
        public double Price { get; set; } = 0;

        [Required(ErrorMessage = "Image is required")]
        public IFormFile? ImageFile { get; set; }    
    }
}
