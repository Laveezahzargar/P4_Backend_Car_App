using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using P4_Backend_Car_App.DTOs;

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
            var manufacturers = await _context.Manufacturers
            .Select(m => new ManufacturerDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description
            })
            .ToListAsync();
            return Ok(new { statusCode = 200, message = "Manufacturers retrieved successfully", data = manufacturers });
        }

        // GET: api/manufacturers/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var manufacturer = await _context.Manufacturers
            .Where(m => m.Id == id)
            .Select(m => new ManufacturerDto
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description
            })
            .FirstOrDefaultAsync();

            if (manufacturer == null)
                return NotFound();

            return Ok(new { statusCode = 200, message = "Manufacturer retrieved successfully", data = manufacturer });
        }

        // POST: api/manufacturers
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ManufacturerCreateUpdateDto m)
        {
            m.Name = m.Name.Trim();

            bool exists = await _context.Manufacturers
                .AnyAsync(x => x.Name.ToLower() == m.Name.ToLower());

            if (exists)
            {
                return BadRequest("Manufacturer already exists");
            }
            var manufacturer = new Manufacturer
            {
                Name = m.Name,
                Description = m.Description
            };
            _context.Manufacturers.Add(manufacturer);
            await _context.SaveChangesAsync();
            return Ok(new { statusCode = 201, message = "Manufacturer added successfully", data = manufacturer.Id });
        }

        // PUT: api/manufacturers/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ManufacturerCreateUpdateDto m)
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

            return Ok(new { statusCode = 200, message = "Manufacturer updated successfully", data = existing.Id });
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

            return Ok(new { statusCode = 200, message = "Manufacturer deleted successfully", data = manufacturer.Id });
        }
    }
}
