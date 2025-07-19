using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models;

public partial class Payment
{
    [Key]
    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [Column("OrderID")]
    public int? OrderId { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Amount { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? PaidAt { get; set; }

    [Column(TypeName = "varchar(20)")]
    [StringLength(20)]
    public string? Status { get; set; }


    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual Order? Order { get; set; }
}
