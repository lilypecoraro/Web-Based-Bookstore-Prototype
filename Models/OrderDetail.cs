using System.ComponentModel.DataAnnotations;

namespace Pecoraro_Lillian_HW5.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, Int32.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        // Price at the time of order
        [Display(Name = "Product Price")]
        [DataType(DataType.Currency)]
        public decimal ProductPrice { get; set; }

        // Quantity × ProductPrice
        [Display(Name = "Extended Price")]
        [DataType(DataType.Currency)]
        public decimal ExtendedPrice { get; set; }

        // Navigation: belongs to ONE Order
        public Order Order { get; set; }

        // Navigation: belongs to ONE Product
        public Product Product { get; set; }
    }
}