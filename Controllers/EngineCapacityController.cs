using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using Microsoft.EntityFrameworkCore;

namespace P4_Backend_Car_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineCapacityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EngineCapacityController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/EngineCapacities
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.EngineCapacities.ToListAsync();
            return Ok(data);
        }

        // GET: api/EngineCapacities/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.EngineCapacities.FindAsync(id);

            if (item == null)
                return NotFound();

            return Ok(item);
        }

        // POST: api/EngineCapacities
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EngineCapacity e)
        {
            e.Name = e.Name.Trim();
            e.Capacity = e.Capacity.Trim();

            bool exists = await _context.EngineCapacities
           .AnyAsync(x =>
             (x.Name.ToLower() == e.Name.ToLower()
            || x.Capacity.ToLower() == e.Capacity.ToLower()));

            if (exists)
            {
                return BadRequest("Engine name or capacity already exists");
            }
            _context.EngineCapacities.Add(e);
            await _context.SaveChangesAsync();

            return Ok(e);
        }

        // PUT: api/EngineCapacities/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EngineCapacity e)
        {
            var existing = await _context.EngineCapacities.FindAsync(id);

            if (existing == null)
                return NotFound();

            e.Name = e.Name.Trim();
            e.Capacity = e.Capacity.Trim();

            bool exists = await _context.EngineCapacities
           .AnyAsync(x =>
             (x.Name.ToLower() == e.Name.ToLower()
            || x.Capacity.ToLower() == e.Capacity.ToLower())
            && x.Id != id);

            if (exists)
            {
                return BadRequest("Engine name or capacity already exists");
            }

            // update fields
            existing.Name = e.Name;
            existing.Capacity = e.Capacity;
            existing.Description = e.Description;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/EngineCapacities/1
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

            return Ok();
        }
    }
}
