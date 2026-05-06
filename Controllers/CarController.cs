using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using Microsoft.EntityFrameworkCore;

namespace P4_Backend_Car_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CarController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Cars
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cars = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.EngineCapacity)
                .ToListAsync();

            return Ok(cars);
        }

        // GET: api/Cars/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.EngineCapacity)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (car == null)
                return NotFound();

            return Ok(car);
        }

        // POST: api/Cars
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Car car)
        {
            // validate foreign keys
            var manufacturerExists = await _context.Manufacturers.AnyAsync(m => m.Id == car.ManufacturerId);
            var engineExists = await _context.EngineCapacities.AnyAsync(e => e.Id == car.EngineCapacityId);

            if (!manufacturerExists || !engineExists)
                return BadRequest("Invalid ManufacturerId or EngineCapacityId");

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            return Ok(car);
        }

        // PUT: api/Cars/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Car car)
        {
            var existing = await _context.Cars.FindAsync(id);

            if (existing == null)
                return NotFound();

            // update fields
            existing.Name = car.Name;
            existing.ManufacturerId = car.ManufacturerId;
            existing.EngineCapacityId = car.EngineCapacityId;
            existing.FuelType = car.FuelType;
            existing.Transmission = car.Transmission;
            existing.Price = car.Price;
            existing.Year = car.Year;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/Cars/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var car = await _context.Cars.FindAsync(id);

            if (car == null)
                return NotFound();

            _context.Cars.Remove(car);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
