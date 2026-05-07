using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;

namespace P4_Backend_Car_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManufacturerController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/manufacturers
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Manufacturers.ToListAsync());
        }

        // GET: api/manufacturers/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.Manufacturers.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST: api/manufacturers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]Manufacturer m)
        {
            m.Name = m.Name.Trim();

            bool exists = await _context.Manufacturers
                .AnyAsync(x => x.Name.ToLower() == m.Name.ToLower());

            if (exists)
            {
                return BadRequest("Manufacturer already exists");
            }
            _context.Manufacturers.Add(m);
            await _context.SaveChangesAsync();
            return Ok(m);
        }

        // PUT: api/manufacturers/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Manufacturer m)
        {
            var existing = await _context.Manufacturers.FindAsync(id);

            if (existing == null)
                return NotFound();

            m.Name = m.Name.Trim();

            bool exists = await _context.Manufacturers
                .AnyAsync(x =>
                    x.Name.ToLower() == m.Name.ToLower()
                    && x.Id != id);

            if (exists)
            {
                return BadRequest("Manufacturer already exists");
            }
            // update only what you need
            existing.Name = m.Name;
            existing.Description = m.Description;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/manufacturers/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var manufacturer = await _context.Manufacturers
                .Include(m => m.Cars)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (manufacturer == null)
                return NotFound();

            if (manufacturer.Cars != null && manufacturer.Cars.Any())
            {
                return BadRequest("Cannot delete manufacturer because it is linked to cars.");
            }

            _context.Manufacturers.Remove(manufacturer);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
