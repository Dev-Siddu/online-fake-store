using Microsoft.AspNetCore.Http;
using Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Models.DTO
{
    public class AddProductDTO
    {
        [Required(ErrorMessage = "Enter Name")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Enter Name")]
        public string? Description { get; set; } = null;

        [Required(ErrorMessage = "Enter price")]
        public double Price { get; set; } = 0;

        [Required(ErrorMessage = "Image is required")]
        public IFormFile? ImageFile { get; set; }   
        
    }
    public static class AddProductDTOExtensions
    {
        public static Product toProduct(this AddProductDTO prod, long ID, string imagePath)
        {
            Product product = new Product();
            product.ID = ID;
            product.Name = prod.Name;
            product.Description = prod.Description;
            product.Price = prod.Price;
            product.ImagePath = "http://localhost:5035/" + "Product_Images/" + imagePath;
            product.Rating = null;
            return product;
        }
    }

}



