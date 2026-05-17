using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBayt.Data;
using SmartBayt.DTOs;
using SmartBayt.Models;

namespace SmartBayt.Controllers;

// ─── CATEGORIES ──────────────────────────────────────────────────────────────
[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDbContext db) : ControllerBase
{
    static CategoryResponse ToDto(Category c) => new(c.Id, c.Name, c.Slug, c.Icon, c.DisplayOrder, c.CreatedAt);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Categories.OrderBy(c => c.DisplayOrder).Select(c => ToDto(c)).ToListAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var c = await db.Categories.FindAsync(id);
        return c is null ? NotFound() : Ok(ToDto(c));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CategoryRequest req)
    {
        if (await db.Categories.AnyAsync(c => c.Slug == req.Slug))
            return BadRequest(new { message = "الـ Slug مستخدم بالفعل" });

        var c = new Category { Name = req.Name, Slug = req.Slug, Icon = req.Icon, DisplayOrder = req.DisplayOrder };
        db.Categories.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, ToDto(c));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, CategoryRequest req)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        c.Name = req.Name; c.Slug = req.Slug; c.Icon = req.Icon; c.DisplayOrder = req.DisplayOrder;
        await db.SaveChangesAsync();
        return Ok(ToDto(c));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ─── REVIEWS ─────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/reviews")]
public class ReviewsController(AppDbContext db) : ControllerBase
{
    static ReviewResponse ToDto(Review r) => new(r.Id, r.ProductId, r.UserId, r.AuthorName, r.Rating, r.Comment, r.Approved, r.CreatedAt);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? productId, [FromQuery] bool? approved)
    {
        var q = db.Reviews.AsQueryable();
        if (productId.HasValue) q = q.Where(r => r.ProductId == productId);
        if (approved.HasValue) q = q.Where(r => r.Approved == approved.Value);
        return Ok(await q.OrderByDescending(r => r.CreatedAt).Select(r => ToDto(r)).ToListAsync());
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(ReviewRequest req)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = new Review
        {
            ProductId = req.ProductId,
            UserId = userId,
            AuthorName = req.AuthorName,
            Rating = req.Rating,
            Comment = req.Comment,
            Approved = false
        };
        db.Reviews.Add(r);
        await db.SaveChangesAsync();
        return Ok(ToDto(r));
    }

    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var r = await db.Reviews.FindAsync(id);
        if (r is null) return NotFound();
        r.Approved = true;
        await db.SaveChangesAsync();
        return Ok(ToDto(r));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var r = await db.Reviews.FindAsync(id);
        if (r is null) return NotFound();
        db.Reviews.Remove(r);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ─── COUPONS ─────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/coupons")]
public class CouponsController(AppDbContext db) : ControllerBase
{
    static CouponResponse ToDto(Coupon c) => new(c.Id, c.Code, c.DiscountType, c.DiscountValue, c.MaxUses, c.UsedCount, c.ExpiresAt, c.IsActive, c.CreatedAt);

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.Coupons.OrderByDescending(c => c.CreatedAt).Select(c => ToDto(c)).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(CouponRequest req)
    {
        if (await db.Coupons.AnyAsync(c => c.Code == req.Code.ToUpper()))
            return BadRequest(new { message = "الكود مستخدم بالفعل" });

        var c = new Coupon
        {
            Code = req.Code.ToUpper(),
            DiscountType = req.DiscountType,
            DiscountValue = req.DiscountValue,
            MaxUses = req.MaxUses,
            ExpiresAt = req.ExpiresAt,
            IsActive = req.IsActive
        };
        db.Coupons.Add(c);
        await db.SaveChangesAsync();
        return Ok(ToDto(c));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, CouponRequest req)
    {
        var c = await db.Coupons.FindAsync(id);
        if (c is null) return NotFound();
        c.Code = req.Code.ToUpper(); c.DiscountType = req.DiscountType;
        c.DiscountValue = req.DiscountValue; c.MaxUses = req.MaxUses;
        c.ExpiresAt = req.ExpiresAt; c.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(ToDto(c));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var c = await db.Coupons.FindAsync(id);
        if (c is null) return NotFound();
        db.Coupons.Remove(c);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate(ValidateCouponRequest req)
    {
        var coupon = await db.Coupons.FirstOrDefaultAsync(c =>
            c.Code == req.Code.ToUpper() && c.IsActive &&
            (c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow) &&
            (c.MaxUses == null || c.UsedCount < c.MaxUses));

        if (coupon is null)
            return Ok(new ValidateCouponResponse(false, "الكود غير صحيح أو منتهي الصلاحية", 0, req.CartTotal));

        var discount = coupon.DiscountType == "percent"
            ? req.CartTotal * coupon.DiscountValue / 100
            : coupon.DiscountValue;

        return Ok(new ValidateCouponResponse(true, null, discount, req.CartTotal - discount));
    }
}

// ─── FAQS ────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/faqs")]
public class FaqsController(AppDbContext db) : ControllerBase
{
    static FaqResponse ToDto(Faq f) => new(f.Id, f.Question, f.Answer, f.DisplayOrder, f.IsActive, f.CreatedAt);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? active)
    {
        var q = db.Faqs.AsQueryable();
        if (active.HasValue) q = q.Where(f => f.IsActive == active.Value);
        return Ok(await q.OrderBy(f => f.DisplayOrder).Select(f => ToDto(f)).ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(FaqRequest req)
    {
        var f = new Faq { Question = req.Question, Answer = req.Answer, DisplayOrder = req.DisplayOrder, IsActive = req.IsActive };
        db.Faqs.Add(f);
        await db.SaveChangesAsync();
        return Ok(ToDto(f));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, FaqRequest req)
    {
        var f = await db.Faqs.FindAsync(id);
        if (f is null) return NotFound();
        f.Question = req.Question; f.Answer = req.Answer;
        f.DisplayOrder = req.DisplayOrder; f.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(ToDto(f));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var f = await db.Faqs.FindAsync(id);
        if (f is null) return NotFound();
        db.Faqs.Remove(f);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ─── BLOG ────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/blog")]
public class BlogController(AppDbContext db) : ControllerBase
{
    static BlogPostResponse ToDto(BlogPost b) => new(b.Id, b.Title, b.Slug, b.Excerpt, b.Content, b.CoverImage, b.Author, b.IsPublished, b.CreatedAt, b.UpdatedAt);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? published)
    {
        var q = db.BlogPosts.AsQueryable();
        if (published.HasValue) q = q.Where(b => b.IsPublished == published.Value);
        return Ok(await q.OrderByDescending(b => b.CreatedAt).Select(b => ToDto(b)).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var b = await db.BlogPosts.FindAsync(id);
        return b is null ? NotFound() : Ok(ToDto(b));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(BlogPostRequest req)
    {
        var b = new BlogPost { Title = req.Title, Slug = req.Slug, Excerpt = req.Excerpt, Content = req.Content, CoverImage = req.CoverImage, Author = req.Author, IsPublished = req.IsPublished };
        db.BlogPosts.Add(b);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = b.Id }, ToDto(b));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, BlogPostRequest req)
    {
        var b = await db.BlogPosts.FindAsync(id);
        if (b is null) return NotFound();
        b.Title = req.Title; b.Slug = req.Slug; b.Excerpt = req.Excerpt;
        b.Content = req.Content; b.CoverImage = req.CoverImage;
        b.Author = req.Author; b.IsPublished = req.IsPublished;
        await db.SaveChangesAsync();
        return Ok(ToDto(b));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var b = await db.BlogPosts.FindAsync(id);
        if (b is null) return NotFound();
        db.BlogPosts.Remove(b);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ─── JOBS ────────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/jobs")]
public class JobsController(AppDbContext db) : ControllerBase
{
    static JobResponse ToDto(Job j) => new(j.Id, j.Title, j.Department, j.Location, j.JobType, j.Description, j.Requirements, j.IsActive, j.CreatedAt);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? active)
    {
        var q = db.Jobs.AsQueryable();
        if (active.HasValue) q = q.Where(j => j.IsActive == active.Value);
        return Ok(await q.OrderByDescending(j => j.CreatedAt).Select(j => ToDto(j)).ToListAsync());
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(JobRequest req)
    {
        var j = new Job { Title = req.Title, Department = req.Department, Location = req.Location, JobType = req.JobType, Description = req.Description, Requirements = req.Requirements, IsActive = req.IsActive };
        db.Jobs.Add(j);
        await db.SaveChangesAsync();
        return Ok(ToDto(j));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, JobRequest req)
    {
        var j = await db.Jobs.FindAsync(id);
        if (j is null) return NotFound();
        j.Title = req.Title; j.Department = req.Department; j.Location = req.Location;
        j.JobType = req.JobType; j.Description = req.Description;
        j.Requirements = req.Requirements; j.IsActive = req.IsActive;
        await db.SaveChangesAsync();
        return Ok(ToDto(j));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var j = await db.Jobs.FindAsync(id);
        if (j is null) return NotFound();
        db.Jobs.Remove(j);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

// ─── SITE SETTINGS ───────────────────────────────────────────────────────────
[ApiController]
[Route("api/settings")]
public class SettingsController(AppDbContext db) : ControllerBase
{
    static SettingResponse ToDto(SiteSetting s) => new(s.Key, s.Value, s.UpdatedAt);

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.SiteSettings.Select(s => ToDto(s)).ToListAsync());

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var s = await db.SiteSettings.FindAsync(key);
        return s is null ? NotFound() : Ok(ToDto(s));
    }

    [HttpPut("{key}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Upsert(string key, SettingRequest req)
    {
        var s = await db.SiteSettings.FindAsync(key);
        if (s is null) { s = new SiteSetting { Key = key }; db.SiteSettings.Add(s); }
        s.Value = req.Value;
        await db.SaveChangesAsync();
        return Ok(ToDto(s));
    }
}

// ─── CONTACT ─────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/contact")]
public class ContactController(AppDbContext db) : ControllerBase
{
    static ContactResponse ToDto(ContactMessage m) => new(m.Id, m.Name, m.Email, m.Phone, m.Subject, m.Message, m.Status, m.CreatedAt);

    [HttpPost]
    public async Task<IActionResult> Send(ContactRequest req)
    {
        var m = new ContactMessage { Name = req.Name, Email = req.Email, Phone = req.Phone, Subject = req.Subject, Message = req.Message, AttachmentUrl = req.AttachmentUrl };
        db.ContactMessages.Add(m);
        await db.SaveChangesAsync();
        return Ok(new { message = "تم إرسال رسالتك بنجاح" });
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll() =>
        Ok(await db.ContactMessages.OrderByDescending(m => m.CreatedAt).Select(m => ToDto(m)).ToListAsync());
}

// ─── DASHBOARD ───────────────────────────────────────────────────────────────
[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "admin")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var since = DateTime.UtcNow.AddDays(-6);

        var products = await db.Products.CountAsync();
        var orders = await db.Orders.CountAsync();
        var customers = await db.Users.CountAsync();
        var revenue = await db.Orders.SumAsync(o => (decimal?)o.Total) ?? 0;

        var recent = await db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Take(5)
            .Select(o => new RecentOrderResponse(o.Id, o.CustomerName, o.Total, o.Status, o.CreatedAt))
            .ToListAsync();

        var salesRaw = await db.Orders
            .Where(o => o.CreatedAt >= since)
            .Select(o => new { Date = o.CreatedAt.Date, o.Total })
            .ToListAsync();

        var salesByDay = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-6 + i))
            .Select(d => new SalesByDayResponse(
                d.ToString("MM-dd"),
                salesRaw.Where(o => o.Date == d).Sum(o => o.Total)
            )).ToList();

        var byCategory = await db.Products
            .GroupBy(p => p.Category)
            .Select(g => new ProductsByCategoryResponse(g.Key, g.Count()))
            .ToListAsync();

        return Ok(new DashboardResponse(products, orders, customers, revenue, recent, salesByDay, byCategory));
    }
}

// ─── UPLOAD ──────────────────────────────────────────────────────────────────
[ApiController]
[Route("api/upload")]
[Authorize(Roles = "admin")]
public class UploadController(IWebHostEnvironment env) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "لم يتم اختيار ملف" });

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { message = "نوع الملف غير مسموح" });

        var uploads = Path.Combine(env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploads);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploads, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream);

        var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        return Ok(new { url });
    }
}

// ─── CUSTOMERS ───────────────────────────────────────────────────────────────
[ApiController]
[Route("api/auth")]
public class CustomersController(AppDbContext db) : ControllerBase
{
    [HttpGet("users")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsers() =>
        Ok(await db.Users.OrderByDescending(u => u.CreatedAt)
            .Select(u => new { u.Id, u.Email, u.FullName, u.Role, u.CreatedAt })
            .ToListAsync());
}