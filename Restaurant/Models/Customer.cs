using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Models;

public partial class Customer
{
    [Key]
    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(15)]
    public string Phone { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(100)]
    public string Password { get; set; } = "";

    //SuperAdmin 

    public bool IsSuperAdmin { get; set; } = false;

    //Admin 
    public bool IsAdmin { get; set; } = false;

    //Staff

    public bool IsStaff { get; set; } = false;

    [InverseProperty("Customer")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
