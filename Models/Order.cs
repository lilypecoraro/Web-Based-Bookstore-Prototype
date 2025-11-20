using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Pecoraro_Lillian_HW5.Models
{
    public class Order
    {
        public const double TAX_RATE = 0.0825;

        public int OrderID { get; set; }

        [Display(Name = "Order Number")]
        public int OrderNumber { get; set; }

        [Display(Name = "Order Date")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Notes")]
        public string? OrderNotes { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        // 👇 THIS MUST BE HERE NOW — and will actually work
        [BindNever]
        public AppUser Customer { get; set; }


        // ----------- Computed Properties (NOT stored in database) -----------

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Subtotal =>
            OrderDetails?.Sum(od => od.ExtendedPrice) ?? 0m;

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SalesTax =>
            Subtotal * (decimal)TAX_RATE;

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Total =>
            Subtotal + SalesTax;
    }
}