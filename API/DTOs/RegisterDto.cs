using System;
using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required] public string Username { get; set; }
        [Required] public string KnownAs { get; set; }
        //[Required] public string Gender { get; set; }
        [Required] public DateTime DateOfBirth { get; set; }
        //[Required] public string City { get; set; }
        //[Required] public string Country { get; set; }
        [Required]
        [EmailAddress]
         public string Email {get; set;}
        [Required]
        [StringLength(16, MinimumLength = 4)]
        public string Password { get; set; }
        public string ClientURI { get; set; }
        
    }
}