using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pecoraro_Lillian_HW5.Models
{
    public enum ProductType
    {
        NewHardback,
        NewPaperback,
        UsedHardback,
        UsedPaperback,
        Other
    }

    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        [Range(0.00, 1000000.00, ErrorMessage = "Price must be non-negative.")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Product Type")]
        public ProductType ProductType { get; set; }

        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();
    }
}