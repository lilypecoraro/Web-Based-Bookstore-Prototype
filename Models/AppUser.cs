using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Pecoraro_Lillian_HW5.Models
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        // Navigation: A user can have many orders
        public List<Order> Orders { get; set; } = new List<Order>();

    }
}