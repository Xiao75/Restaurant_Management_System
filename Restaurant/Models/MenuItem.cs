using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models;

public partial class MenuItem
{
    [Key]
    [Column("ItemID")]
    public int ItemId { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    [StringLength(50)]
    public string? Category { get; set; }

    public bool Available { get; set; }

    [StringLength(255)]
    public string? ImagePath { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
