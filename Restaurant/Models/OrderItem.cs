using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models;

public partial class OrderItem
{
    [Key]
    [Column("OrderItemID")]
    public int OrderItemId { get; set; }

    [Column("OrderID")]
    [Required] // Not nullable
    public int OrderId { get; set; }

    [Column("ItemID")]
    [Required] // Not nullable
    public int ItemId { get; set; }

    [Required] // Quantity is always needed
    public int Quantity { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("OrderItems")]
    public virtual MenuItem Item { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order Order { get; set; }
}
