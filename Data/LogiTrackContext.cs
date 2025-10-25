using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LogiTrack.Models;

namespace LogiTrack.Data
{
    public class LogiTrackContext : IdentityDbContext<ApplicationUser>
    {
        public LogiTrackContext(DbContextOptions<LogiTrackContext> options) : base(options) { }

        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Optional: unique constraints or indexes
            builder.Entity<InventoryItem>()
                .HasIndex(i => new { i.Name, i.Location })
                .IsUnique(false);

            builder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.InventoryItem)
                .WithMany()
                .HasForeignKey(oi => oi.InventoryItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
