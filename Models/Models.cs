using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBayt.Models;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }

    // --- الحقول الجديدة لاستعادة كلمة المرور ---
    public string? ResetCode { get; set; }
    public DateTime? ResetCodeExpiry { get; set; }

    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}

public class Category
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Name { get; set; } = "";
    [Required] public string Slug { get; set; } = "";
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Name { get; set; } = "";
    public string? Slug { get; set; }
    [Required] public string Category { get; set; } = "";
    [Column(TypeName = "decimal(10,2)")] public decimal Price { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal? OriginalPrice { get; set; }
    [Column(TypeName = "decimal(2,1)")] public decimal Rating { get; set; } = 0;
    public int Reviews { get; set; } = 0;
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string Images { get; set; } = "[]";
    public string? Description { get; set; }
    public int Stock { get; set; } = 0;
    public string Features { get; set; } = "[]";
    public string Specs { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Review> ProductReviews { get; set; } = [];
}

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    [Required] public string CustomerName { get; set; } = "";
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ShippingAddress { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal Total { get; set; }
    public string Status { get; set; } = "pending";
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    [Required] public string ProductName { get; set; } = "";
    [Column(TypeName = "decimal(10,2)")] public decimal Price { get; set; }
    public int Quantity { get; set; } = 1;
}

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    [Required] public string AuthorName { get; set; } = "";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool Approved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Coupon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Code { get; set; } = "";
    public string DiscountType { get; set; } = "percent";
    [Column(TypeName = "decimal(10,2)")] public decimal DiscountValue { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; } = 0;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Faq
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Question { get; set; } = "";
    [Required] public string Answer { get; set; } = "";
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class BlogPost
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Title { get; set; } = "";
    [Required] public string Slug { get; set; } = "";
    public string? Excerpt { get; set; }
    [Required] public string Content { get; set; } = "";
    public string? CoverImage { get; set; }
    public string? Author { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Title { get; set; } = "";
    public string? Department { get; set; }
    public string? Location { get; set; }
    public string? JobType { get; set; }
    [Required] public string Description { get; set; } = "";
    public string? Requirements { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SiteSetting
{
    [Key] public string Key { get; set; } = "";
    [Required] public string Value { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class ContactMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public string Name { get; set; } = "";
    [Required] public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    [Required] public string Message { get; set; } = "";
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = "new";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}