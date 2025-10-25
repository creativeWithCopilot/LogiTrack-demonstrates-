using LogiTrack.Contracts;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly LogiTrackContext _context;

        public OrderController(LogiTrackContext context)
        {
            _context = context;
        }

        // GET /api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderReadDto>>> GetAll()
        {
            var sw = Stopwatch.StartNew();

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.InventoryItem)
                .OrderByDescending(o => o.DatePlaced)
                .Select(o => new OrderReadDto
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    DatePlaced = o.DatePlaced,
                    Items = o.Items.Select(oi => new OrderItemReadDto
                    {
                        InventoryItemId = oi.InventoryItemId,
                        ItemName = oi.InventoryItem!.Name,
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .ToListAsync();

            sw.Stop();
            Response.Headers.Add("X-Elapsed-ms", sw.ElapsedMilliseconds.ToString());
            return Ok(orders);
        }

        // GET /api/orders/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderReadDto>> GetById(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.InventoryItem)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            var dto = new OrderReadDto
            {
                OrderId = order.OrderId,
                CustomerName = order.CustomerName,
                DatePlaced = order.DatePlaced,
                Items = order.Items.Select(oi => new OrderItemReadDto
                {
                    InventoryItemId = oi.InventoryItemId,
                    ItemName = oi.InventoryItem!.Name,
                    Quantity = oi.Quantity
                }).ToList()
            };
            return Ok(dto);
        }

        // POST /api/orders
        [HttpPost]
        public async Task<ActionResult<OrderReadDto>> Create([FromBody] OrderCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest("CustomerName is required");
            if (dto.Items == null || dto.Items.Count == 0)
                return BadRequest("At least one item is required");
            if (dto.Items.Any(i => i.Quantity <= 0))
                return BadRequest("Item quantities must be positive");

            // Validate item existence in one query
            var itemIds = dto.Items.Select(i => i.InventoryItemId).Distinct().ToList();
            var existingItems = await _context.InventoryItems
                .Where(i => itemIds.Contains(i.ItemId))
                .ToListAsync();

            if (existingItems.Count != itemIds.Count)
                return BadRequest("One or more InventoryItemIds do not exist");

            var order = new Order
            {
                CustomerName = dto.CustomerName.Trim(),
                DatePlaced = dto.DatePlaced ?? DateTime.UtcNow
            };

            // Build order items
            foreach (var line in dto.Items)
            {
                order.Items.Add(new OrderItem
                {
                    InventoryItemId = line.InventoryItemId,
                    Quantity = line.Quantity
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Return with eager-loaded details
            var created = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.InventoryItem)
                .FirstAsync(o => o.OrderId == order.OrderId);

            var read = new OrderReadDto
            {
                OrderId = created.OrderId,
                CustomerName = created.CustomerName,
                DatePlaced = created.DatePlaced,
                Items = created.Items.Select(oi => new OrderItemReadDto
                {
                    InventoryItemId = oi.InventoryItemId,
                    ItemName = oi.InventoryItem!.Name,
                    Quantity = oi.Quantity
                }).ToList()
            };

            return CreatedAtAction(nameof(GetById), new { id = read.OrderId }, read);
        }

        // DELETE /api/orders/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.Orders.FindAsync(id);
            if (entity == null) return NotFound();

            _context.Orders.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
