using Microsoft.AspNetCore.Http;
using Models.DTO;
using Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO
{
    public class UserRegistrationRequest
    {
        [Required(ErrorMessage = "Enter Name")]
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Enter Email")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }  // UserName

        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone number")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Enter Password")]
        [Display(Name = "Password")]

        public string? Password { get; set; }

        [Compare("Password",ErrorMessage = "Password and confirm password do not match")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Profile Image")]
        public IFormFile? Image { get; set; }
    }
}

public static class ConvertToUser
{
    public static User ToUser(this UserRegistrationRequest registrationRequest, string fileName)
    {
        // getting the hased password
        User user = new User();
        user.Name = registrationRequest.Name;
        user.Email = registrationRequest.Email;
        user.Password = registrationRequest.Password;
        user.Phone = registrationRequest.Phone ?? "";
        user.ImageName = (string.IsNullOrEmpty(fileName)) ? "" : fileName;
        user.Role = "user";
        return user;
    }
}
