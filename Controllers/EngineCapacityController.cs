using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace P4_Backend_Car_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EngineCapacityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EngineCapacityController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EngineCapacities
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EngineCapacities
                .Select(e => new EngineCapacityDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    Capacity = e.CapacityCc
                }).ToListAsync();
            return Ok(new { statusCode = 200, message = "Engines retrieved successfully", data = data });
        }

        // GET: api/EngineCapacities/1
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.EngineCapacities
               .Where(e => e.Id == id)
               .Select(e => new EngineCapacityDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        Description = e.Description,
                        Capacity = e.CapacityCc
                    })
               .FirstOrDefaultAsync();

            if (item == null)
                return NotFound();

            return Ok(new { statusCode = 200, message = "Engine retrieved successfully", data = item });
        }

        // POST: api/EngineCapacities
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] EngineCreateUpdateDto e)
        {
            e.Name = e.Name.Trim();
            e.Capacity = e.Capacity.Trim();

            bool exists = await _context.EngineCapacities
           .AnyAsync(x =>
             (x.Name.ToLower() == e.Name.ToLower()
            || x.CapacityCc.ToLower() == e.Capacity.ToLower()));

            if (exists)
            {
                return BadRequest("Engine name or capacity already exists");
            }
            var engine = new EngineCapacity
            {
                Name = e.Name,
                CapacityCc = e.Capacity,
                Description = e.Description
            };
            _context.EngineCapacities.Add(engine);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Engine Added successfully", data = engine.Id });
        }

        // PUT: api/EngineCapacities/1
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EngineCreateUpdateDto e)
        {
            var existing = await _context.EngineCapacities.FindAsync(id);

            if (existing == null)
                return NotFound();

            e.Name = e.Name.Trim();
            e.Capacity = e.Capacity.Trim();

            bool exists = await _context.EngineCapacities
           .AnyAsync(x =>
             (x.Name.ToLower() == e.Name.ToLower()
            || x.CapacityCc .ToLower() == e.Capacity.ToLower())
            && x.Id != id);

            if (exists)
            {
                return BadRequest("Engine name or capacity already exists");
            }

            // update fields
            existing.Name = e.Name;
            existing.CapacityCc = e.Capacity;
            existing.Description = e.Description;

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Engine updated successfully",data= existing.Id });
        }

        // DELETE: api/EngineCapacities/1
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var engine = await _context.EngineCapacities
                .Include(e => e.Cars)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (engine == null)
                return NotFound();

            if (engine.Cars != null && engine.Cars.Any())
            {
                return BadRequest("Cannot delete engine capacity because it is linked to cars.");
            }

            _context.EngineCapacities.Remove(engine);

            await _context.SaveChangesAsync();

            return Ok(new {statusCode=200, message = "Engine deleted successfully",data= engine.Id });
        }
    }
}
