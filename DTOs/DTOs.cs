using System.ComponentModel.DataAnnotations;

namespace SmartBayt.DTOs;

// ─── AUTH ────────────────────────────────────────────────
public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    string? FullName
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    string Id,
    string Email,
    string? FullName,
    string Role
);

// ─── PRODUCT ─────────────────────────────────────────────
public record ProductRequest(
    [Required] string Name,
    string? Slug,
    [Required] string Category,
    decimal Price,
    decimal? OriginalPrice,
    decimal Rating,
    int Reviews,
    string? Badge,
    string? Image,
    string? Images,
    string? Description,
    int Stock,
    string? Features,
    string? Specs,
    bool IsActive
);

public record ProductResponse(
    Guid Id,
    string Name,
    string? Slug,
    string Category,
    decimal Price,
    decimal? OriginalPrice,
    decimal Rating,
    int Reviews,
    string? Badge,
    string? Image,
    string Images,
    string? Description,
    int Stock,
    string Features,
    string Specs,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// ─── CATEGORY ────────────────────────────────────────────
public record CategoryRequest(
    [Required] string Name,
    [Required] string Slug,
    string? Icon,
    int DisplayOrder
);

public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Icon,
    int DisplayOrder,
    DateTime CreatedAt
);

// ─── ORDER ───────────────────────────────────────────────
public record PlaceOrderRequest(
    [Required] string CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    string? ShippingAddress,
    string? PaymentMethod,
    string? Notes,
    [Required] List<OrderItemRequest> Items,
    string? CouponCode
);

public record OrderItemRequest(
    Guid? ProductId,
    [Required] string ProductName,
    decimal Price,
    int Quantity
);

public record OrderResponse(
    Guid Id,
    Guid? UserId,
    string CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    string? ShippingAddress,
    decimal Total,
    string Status,
    string? PaymentMethod,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<OrderItemResponse> Items
);

public record OrderItemResponse(
    Guid Id,
    Guid? ProductId,
    string ProductName,
    decimal Price,
    int Quantity
);

public record UpdateStatusRequest([Required] string Status);

// ─── REVIEW ──────────────────────────────────────────────
public record ReviewRequest(
    Guid? ProductId,
    [Required] string AuthorName,
    [Range(1, 5)] int Rating,
    string? Comment
);

public record ReviewResponse(
    Guid Id,
    Guid? ProductId,
    Guid? UserId,
    string AuthorName,
    int Rating,
    string? Comment,
    bool Approved,
    DateTime CreatedAt
);

// ─── COUPON ──────────────────────────────────────────────
public record CouponRequest(
    [Required] string Code,
    string DiscountType,
    decimal DiscountValue,
    int? MaxUses,
    DateTime? ExpiresAt,
    bool IsActive
);

public record CouponResponse(
    Guid Id,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    int? MaxUses,
    int UsedCount,
    DateTime? ExpiresAt,
    bool IsActive,
    DateTime CreatedAt
);

public record ValidateCouponRequest([Required] string Code, decimal CartTotal);
public record ValidateCouponResponse(bool Valid, string? Message, decimal Discount, decimal FinalTotal);

// ─── FAQ ─────────────────────────────────────────────────
public record FaqRequest(
    [Required] string Question,
    [Required] string Answer,
    int DisplayOrder,
    bool IsActive
);

public record FaqResponse(
    Guid Id,
    string Question,
    string Answer,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt
);

// ─── BLOG ────────────────────────────────────────────────
public record BlogPostRequest(
    [Required] string Title,
    [Required] string Slug,
    string? Excerpt,
    [Required] string Content,
    string? CoverImage,
    string? Author,
    bool IsPublished
);

public record BlogPostResponse(
    Guid Id,
    string Title,
    string Slug,
    string? Excerpt,
    string Content,
    string? CoverImage,
    string? Author,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

// ─── JOB ─────────────────────────────────────────────────
public record JobRequest(
    [Required] string Title,
    string? Department,
    string? Location,
    string? JobType,
    [Required] string Description,
    string? Requirements,
    bool IsActive
);

public record JobResponse(
    Guid Id,
    string Title,
    string? Department,
    string? Location,
    string? JobType,
    string Description,
    string? Requirements,
    bool IsActive,
    DateTime CreatedAt
);

// ─── SITE SETTINGS ───────────────────────────────────────
public record SettingRequest([Required] string Key, [Required] string Value);
public record SettingResponse(string Key, string Value, DateTime UpdatedAt);

// ─── CONTACT ─────────────────────────────────────────────
public record ContactRequest(
    [Required] string Name,
    [Required, EmailAddress] string Email,
    string? Phone,
    string? Subject,
    [Required] string Message,
    string? AttachmentUrl
);

public record ContactResponse(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Subject,
    string Message,
    string Status,
    DateTime CreatedAt
);

// ─── DASHBOARD ───────────────────────────────────────────
public record DashboardResponse(
    int TotalProducts,
    int TotalOrders,
    int TotalCustomers,
    decimal TotalRevenue,
    List<RecentOrderResponse> RecentOrders,
    List<SalesByDayResponse> SalesByDay,
    List<ProductsByCategoryResponse> ProductsByCategory
);

public record RecentOrderResponse(
    Guid Id,
    string CustomerName,
    decimal Total,
    string Status,
    DateTime CreatedAt
);

public record SalesByDayResponse(string Date, decimal Total);
public record ProductsByCategoryResponse(string Category, int Count);

// ─── PAGINATION ──────────────────────────────────────────
public record PagedResponse<T>(
    List<T> Data,
    int Total,
    int Page,
    int PageSize,
    int TotalPages
);

// ─── PROFILE & PASSWORD RESET (NEW) ──────────────────────
public record UpdateProfileRequest(string? FullName, string? AvatarUrl, string? CurrentPassword, string? NewPassword);
public record ForgotPasswordRequest([Required, EmailAddress] string Email);
public record ResetPasswordRequest([Required, EmailAddress] string Email, [Required] string Code, [Required, MinLength(6)] string NewPassword);