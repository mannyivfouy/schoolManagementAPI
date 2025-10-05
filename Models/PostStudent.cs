using System.ComponentModel.DataAnnotations;

namespace SchoolManagementAPI.Models
{
    public class PostStudent
    {
        [Required]
        [StringLength(50, MinimumLength =3)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public IFormFile Image { get; set; } = null!;

        [Required]
        [StringLength(50, MinimumLength = 3)]       
        public string ParentName { get; set; } = string.Empty;

        [Required]
        public string ParentContact { get; set; } = string.Empty;
    }
}
