using Microsoft.EntityFrameworkCore;
using WebApplication_StockDataFixx.Models;
using Microsoft.Extensions.Configuration;

namespace WebApplication_StockDataFixx.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        //public DbSet<ManagementItem> ManagementItems { get; set; }
        //public DbSet<WarehouseItem> WarehouseItems { get; set; }
        //public DbSet<ProductionItem> ProductionItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
      
            //modelBuilder.Entity<ManagementItem>()
            //.HasOne(m => m.WarehouseItem)
            //.WithMany()
            //.HasForeignKey(m => m.WarehouseItemId)
            //.OnDelete(DeleteBehavior.Restrict);

            ////     ManagementItem -> ProductionItem (ManyToOne)
            //modelBuilder.Entity<ManagementItem>()
            //.HasOne(m => m.ProductionItem)
            //.WithMany()
            //.HasForeignKey(m => m.ProductionItemId)
            //.OnDelete(DeleteBehavior.Restrict);

        }
    }
}

//// Define DbSet for each entity (model)
//public DbSet<ManagementItem> ManagementItems { get; set; }
//public DbSet<WarehouseItem> WarehouseItems { get; set; }
//public DbSet<ProductionItem> ProductionItems { get; set; }

//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//{
//    if (!optionsBuilder.IsConfigured)
//    {
//        // Get the connection string from appsettings.json
//        var connectionString = _configuration.GetConnectionString("DefaultConnection");
//        optionsBuilder.UseSqlServer(connectionString);
//    }
//}

//protected override void OnModelCreating(ModelBuilder modelBuilder)
//{
//    // Define the relationships (foreign keys) between entities

//    // ManagementItem -> WarehouseItem (ManyToOne)
//    modelBuilder.Entity<ManagementItem>()
//        .HasOne(m => m.WarehouseItem)
//        .WithMany()
//        .HasForeignKey(m => m.WarehouseItemId)
//        .OnDelete(DeleteBehavior.Restrict);

//    // ManagementItem -> ProductionItem (ManyToOne)
//    modelBuilder.Entity<ManagementItem>()
//        .HasOne(m => m.ProductionItem)
//        .WithMany()
//        .HasForeignKey(m => m.ProductionItemId)
//        .OnDelete(DeleteBehavior.Restrict);
//}