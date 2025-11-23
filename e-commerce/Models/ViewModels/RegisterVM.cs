using System.ComponentModel.DataAnnotations;

namespace Cinema.Models.ViewModels
{
    public class RegisterVM
    {
        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

}
