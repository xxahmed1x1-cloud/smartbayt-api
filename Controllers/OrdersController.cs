using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBayt.Data;
using SmartBayt.DTOs;
using SmartBayt.Models;

namespace SmartBayt.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(AppDbContext db) : ControllerBase
{
    static OrderResponse ToDto(Order o) => new(
        o.Id, o.UserId, o.CustomerName, o.CustomerEmail, o.CustomerPhone,
        o.ShippingAddress, o.Total, o.Status, o.PaymentMethod, o.Notes,
        o.CreatedAt, o.UpdatedAt,
        o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.Price, i.Quantity)).ToList()
    );

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderRequest req)
    {
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(req.CouponCode))
        {
            var coupon = await db.Coupons.FirstOrDefaultAsync(c =>
                c.Code == req.CouponCode.ToUpper() && c.IsActive &&
                (c.ExpiresAt == null || c.ExpiresAt > DateTime.UtcNow) &&
                (c.MaxUses == null || c.UsedCount < c.MaxUses));

            if (coupon is null)
                return BadRequest(new { message = "كود الخصم غير صحيح أو منتهي الصلاحية" });

            var subtotal = req.Items.Sum(i => i.Price * i.Quantity);
            discount = coupon.DiscountType == "percent"
                ? subtotal * coupon.DiscountValue / 100
                : coupon.DiscountValue;

            coupon.UsedCount++;
        }

        var subtotalFinal = req.Items.Sum(i => i.Price * i.Quantity);
        var shippingFee = subtotalFinal > 1000 ? 0 : 50;
        var total = subtotalFinal + shippingFee - discount;

        Guid? userId = null;
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(idClaim)) userId = Guid.Parse(idClaim);

        var order = new Order
        {
            UserId = userId,
            CustomerName = req.CustomerName,
            CustomerEmail = req.CustomerEmail,
            CustomerPhone = req.CustomerPhone,
            ShippingAddress = req.ShippingAddress,
            Total = total,
            Status = "pending",
            PaymentMethod = req.PaymentMethod,
            Notes = req.Notes,
            Items = req.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList()
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        await db.Entry(order).Collection(o => o.Items).LoadAsync();
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, ToDto(order));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim is not null && order.UserId.HasValue && order.UserId.ToString() != idClaim
            && User.FindFirst(ClaimTypes.Role)?.Value != "admin")
            return Forbid();

        return Ok(ToDto(order));
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null)
    {
        var q = db.Orders.Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);

        var total = await q.CountAsync();
        var data = await q.OrderByDescending(o => o.CreatedAt)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .Select(o => ToDto(o))
                           .ToListAsync();

        return Ok(new PagedResponse<OrderResponse>(data, total, page, pageSize, (int)Math.Ceiling((double)total / pageSize)));
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMine()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var orders = await db.Orders.Include(o => o.Items)
                             .Where(o => o.UserId == userId)
                             .OrderByDescending(o => o.CreatedAt)
                             .ToListAsync();
        return Ok(orders.Select(ToDto));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusRequest req)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        order.Status = req.Status;
        await db.SaveChangesAsync();
        return Ok(new { message = "تم تحديث الحالة", status = order.Status });
    }
}