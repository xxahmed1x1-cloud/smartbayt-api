using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBayt.Data;
using SmartBayt.DTOs;
using SmartBayt.Models;

namespace SmartBayt.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    static ProductResponse ToDto(Product p) => new(
        p.Id, p.Name, p.Slug, p.Category, p.Price, p.OriginalPrice,
        p.Rating, p.Reviews, p.Badge, p.Image, p.Images, p.Description,
        p.Stock, p.Features, p.Specs, p.IsActive, p.CreatedAt, p.UpdatedAt
    );

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? active = null)
    {
        var q = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category)) q = q.Where(p => p.Category == category);
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));
        if (active.HasValue) q = q.Where(p => p.IsActive == active.Value);

        var total = await q.CountAsync();
        var data = await q.OrderByDescending(p => p.CreatedAt)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .Select(p => ToDto(p))
                           .ToListAsync();

        return Ok(new PagedResponse<ProductResponse>(data, total, page, pageSize, (int)Math.Ceiling((double)total / pageSize)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var p = await db.Products.FindAsync(id);
        return p is null ? NotFound() : Ok(ToDto(p));
    }

    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var p = await db.Products.FirstOrDefaultAsync(x => x.Slug == slug);
        return p is null ? NotFound() : Ok(ToDto(p));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(ProductRequest req)
    {
        var p = new Product
        {
            Name = req.Name,
            Slug = req.Slug,
            Category = req.Category,
            Price = req.Price,
            OriginalPrice = req.OriginalPrice,
            Rating = req.Rating,
            Reviews = req.Reviews,
            Badge = req.Badge,
            Image = req.Image,
            Images = req.Images ?? "[]",
            Description = req.Description,
            Stock = req.Stock,
            Features = req.Features ?? "[]",
            Specs = req.Specs ?? "[]",
            IsActive = req.IsActive
        };
        db.Products.Add(p);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = p.Id }, ToDto(p));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(Guid id, ProductRequest req)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();

        p.Name = req.Name;
        p.Slug = req.Slug;
        p.Category = req.Category;
        p.Price = req.Price;
        p.OriginalPrice = req.OriginalPrice;
        p.Rating = req.Rating;
        p.Reviews = req.Reviews;
        p.Badge = req.Badge;
        p.Image = req.Image;
        p.Images = req.Images ?? "[]";
        p.Description = req.Description;
        p.Stock = req.Stock;
        p.Features = req.Features ?? "[]";
        p.Specs = req.Specs ?? "[]";
        p.IsActive = req.IsActive;

        await db.SaveChangesAsync();
        return Ok(ToDto(p));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}