
using System.ComponentModel.DataAnnotations;

namespace Models.Entities
{
    public class Cart
    {
        [Required(ErrorMessage = "User id required")]
        public int UserID { get; set; }
        [Required(ErrorMessage = "Product id is required")]
        public List<int>? ProdIDs { get; set; }   = new List<int>(); 
    }
}
