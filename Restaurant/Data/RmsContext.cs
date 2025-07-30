using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Restaurant.Models;

namespace Restaurant.Data;

public partial class RmsContext : DbContext
{
    public RmsContext()
    {
    }

    public RmsContext(DbContextOptions<RmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Customer> Customers { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public virtual DbSet<MenuItem> MenuItems { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }

    // ✅ Removed OnConfiguring

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8F1E20B25");
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__MenuItem__727E83EB156104A7");
            entity.Property(e => e.Available).HasDefaultValue(true);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF8F88388D");
            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.Customer).WithMany(p => p.Orders).HasConstraintName("FK__Orders__Customer__3D5E1FD2");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A1767F7706");
            entity.HasOne(d => d.Item).WithMany(p => p.OrderItems).HasConstraintName("FK__OrderItem__ItemI__412EB0B6");
            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems).HasConstraintName("FK__OrderItem__Order__403A8C7D");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaidAt).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.Order)
                .WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__Payments__OrderID");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
