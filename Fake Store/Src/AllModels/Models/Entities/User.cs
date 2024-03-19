
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? ImageName { get; set; }
        public string? Role { get; set; }
    }
}
