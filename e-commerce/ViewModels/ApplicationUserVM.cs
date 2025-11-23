using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Cinema.ViewModels
{
    public class ApplicationUserVM
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        public string? Role { get; set; }
        public IEnumerable<SelectListItem>? Roles { get; set; }
    }
}
