using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Restaurant.Models
{
    public class Order
    {
        //Order type {onile || DineIn }
        public string? Source { get; set; }

        //
        public int? TableNumber { get; set; }


        [Key]
        [Column("OrderID")]
        public int OrderId { get; set; }

        [Column("CustomerID")]
        public int? CustomerId { get; set; }

        [ForeignKey("Address")]
        public int? AddressID { get; set; }

        public string? DeliveryAddress { get; set; }



        [Column(TypeName = "datetime")]
        public DateTime OrderDate { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        //Total Amount of the order item
        [Column(TypeName ="decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }


        // Navigation property to Customer
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        // Navigation property to related order items
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Navigation property to payments
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        public string? InvoiceId { get; set; } = "";

    }
}
