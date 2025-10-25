using LogiTrack.Contracts;
using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly LogiTrackContext _context;
        private readonly IMemoryCache _cache;

        public InventoryController(LogiTrackContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET /api/inventory
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryReadDto>>> Get()
        {
            var sw = Stopwatch.StartNew();

            if (_cache.TryGetValue("inventory:list", out List<InventoryReadDto>? cached))
            {
                sw.Stop();
                Response.Headers.Add("X-Cache", "HIT");
                Response.Headers.Add("X-Elapsed-ms", sw.ElapsedMilliseconds.ToString());
                return Ok(cached);
            }

            var items = await _context.InventoryItems
                .AsNoTracking()
                .OrderBy(i => i.ItemId)
                .Select(i => new InventoryReadDto
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Location = i.Location
                })
                .ToListAsync();

            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            _cache.Set("inventory:list", items, options);

            sw.Stop();
            Response.Headers.Add("X-Cache", "MISS");
            Response.Headers.Add("X-Elapsed-ms", sw.ElapsedMilliseconds.ToString());
            return Ok(items);
        }

        // POST /api/inventory
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<ActionResult<InventoryReadDto>> Create([FromBody] InventoryCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required");
            if (string.IsNullOrWhiteSpace(dto.Location))
                return BadRequest("Location is required");
            if (dto.Quantity < 0)
                return BadRequest("Quantity cannot be negative");

            var entity = new InventoryItem
            {
                Name = dto.Name.Trim(),
                Quantity = dto.Quantity,
                Location = dto.Location.Trim()
            };

            _context.InventoryItems.Add(entity);
            await _context.SaveChangesAsync();

            _cache.Remove("inventory:list"); // invalidate

            var read = new InventoryReadDto
            {
                ItemId = entity.ItemId,
                Name = entity.Name,
                Quantity = entity.Quantity,
                Location = entity.Location
            };
            return CreatedAtAction(nameof(Get), new { id = entity.ItemId }, read);
        }

        // DELETE /api/inventory/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.InventoryItems.FindAsync(id);
            if (entity == null) return NotFound();

            _context.InventoryItems.Remove(entity);
            await _context.SaveChangesAsync();

            _cache.Remove("inventory:list"); // invalidate
            return NoContent();
        }
    }
}
