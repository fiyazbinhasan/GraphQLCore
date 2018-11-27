using GraphQLAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace GraphQLAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
		public DbSet<Item> Items { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItem { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Item>().HasData(
                new Item { ItemId = 1, Barcode = "123", Title = "Headphone", SellingPrice = 50 },
                new Item { ItemId = 2, Barcode = "456", Title = "Keyboard", SellingPrice = 40 },
                new Item { ItemId = 3, Barcode = "789", Title = "Monitor", SellingPrice = 100 }
            );

            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "Jon Doe", BillingAddress = "123 Mainnstreet" }, 
                new Customer { CustomerId = 2, Name = "Jane Doe", BillingAddress = "456 Mainnstreet" }
            );

            modelBuilder.Entity<Order>().HasData(
                new Order { OrderId = 1, Tag = "ORD-123", CreatedAt = DateTime.Today, CustomerId = 1  },
                new Order { OrderId = 2, Tag = "ORD-456", CreatedAt = DateTime.Today.AddDays(-1), CustomerId = 2 }
            );
        }
    }
}