using Microsoft.EntityFrameworkCore;
using SmartBayt.Models;

namespace SmartBayt.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<Faq> Faqs { get; set; }
    public DbSet<BlogPost> BlogPosts { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<SiteSetting> SiteSettings { get; set; }
    public DbSet<ContactMessage> ContactMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        mb.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        mb.Entity<BlogPost>().HasIndex(b => b.Slug).IsUnique();
        mb.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();

        mb.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        mb.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany(p => p.ProductReviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        mb.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Product p) p.UpdatedAt = DateTime.UtcNow;
                if (entry.Entity is Order o) o.UpdatedAt = DateTime.UtcNow;
                if (entry.Entity is BlogPost b) b.UpdatedAt = DateTime.UtcNow;
                if (entry.Entity is SiteSetting s) s.UpdatedAt = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}