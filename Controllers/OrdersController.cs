/*
 * This guarantees data integrity, if stock runs out halfway through processing, the entire transaction rolls back so inventory and orders never fall out of sync
 */


using InventoryApi.Data;
using InventoryApi.DTOs;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrdersController(AppDbContext context)
    {
        _context = context;
    }

    // Fetch paginated, filtered, and dynamically sorted orders.
    [HttpGet]
    public async Task<IActionResult> GetOrders(
    [FromQuery] string? status,
    [FromQuery] string? sortBy = "date",
    [FromQuery] bool isDescending = true,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        IQueryable<Order> query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product);

        //Filtering
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status.ToLower() == status.ToLower());
        }

        //Dynamic Sorting
        query = (sortBy?.ToLower(), isDescending) switch
        {
            ("amount", true) => query.OrderByDescending(o => o.TotalAmount),
            ("amount", false) => query.OrderBy(o => o.TotalAmount),
            ("date", false) => query.OrderBy(o => o.OrderDate),
            _ => query.OrderByDescending(o => o.OrderDate)
        };

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderResponseDto(
                o.OrderId,
                o.CustomerId,
                $"{o.Customer.FirstName}{o.Customer.LastName}",
                o.OrderDate,
                o.TotalAmount,
                o.Status,
                o.OrderItems.Select(oi => new OrderItemResponseDto(
                    oi.ProductId,
                    oi.Product.Name,
                    oi.Quantity,
                    oi.UnitPrice
                    )).ToList()
                ))
                .ToListAsync();
        return Ok(orders);
    }

    //Create a transaction order with inventory stock reduction
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == request.CustomerId);
            if (!customerExists) return BadRequest($"Customer ID {request.CustomerId} not found.");

            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionaryAsync(p => p.ProductId);

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in request.Items)
            {
                if (!products.TryGetValue(item.ProductId, out var product))
                    return BadRequest($"Product ID {item.ProductId} not found.");

                if (product.StockQuantity < item.Quantity)
                    return BadRequest($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}");

                // Reduce inventory stock
                product.StockQuantity -= item.Quantity;
                totalAmount += product.Price * item.Quantity;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                });
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Status = "Completed",
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetOrders), new { id = order.OrderId }, order.OrderId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Transaction failed: {ex.Message}");
        }
    }

    //Get dynamic business analytics summary
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalyticsSummary()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        var avgOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0m;

        var topProducts = await _context.OrderItems
            .AsNoTracking()
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopSellingProductDto(
                g.Key.ProductId,
                g.Key.Name,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.Quantity * oi.UnitPrice)
            ))
            .OrderByDescending(p => p.TotalUnitsSold)
            .Take(5)
            .ToListAsync();
        return Ok(new AnalyticsSummaryDto(totalRevenue, totalOrders, Math.Round(avgOrderValue, 2), topProducts));

    }
}
