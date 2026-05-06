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
            var item = await _context.Manufacturers.FindAsync(id);
            if (item == null) return NotFound();

            _context.Manufacturers.Remove(item);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
