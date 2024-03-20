using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Enter Email or Phone number")]
        [Display(Name = "Email or Phone")]
        public string? EmailOrPhone { get; set; }
        
        [Required(ErrorMessage = "Enter password")]
        [Display(Name = "Enter password")]
        public string? Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
