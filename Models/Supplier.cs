using System.ComponentModel.DataAnnotations;

namespace Pecoraro_Lillian_HW5.Models
{
    public class Supplier
    {
        public int SupplierID { get; set; }

        [Required]
        public string SupplierName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        // Many-to-many: Supplier ⇄ Product
        public List<Product> Products { get; set; } = new List<Product>();
    }
}