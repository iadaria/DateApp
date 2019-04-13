using System.ComponentModel.DataAnnotations;

namespace Acquaintance.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username {get; set;}
        
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage ="You must specify password between 4 and 8 characters")]
        public string Password { get; set;}
        [Required]
        public string Gender { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public System.DateTime DateOfBirth { get; set; }
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastActive { get; set; }
        public UserForRegisterDto()
        {
            Created = System.DateTime.Now;
            LastActive = System.DateTime.Now;
        }

    }
}