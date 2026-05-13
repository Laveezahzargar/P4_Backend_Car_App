using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.DTOs;
using P4_Backend_Car_App.Services;
using P4_Backend_Car_App.Interfaces;
using P4_Backend_Car_App.Types;

namespace P4_Backend_Car_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public CarController(AppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: api/Cars
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cars = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.EngineCapacity)
                .Select(c => new CarDto
                {
                    Id = c.Id,
                    Name = c.Name,

                    ManufacturerId = c.ManufacturerId,
                    Manufacturer = c.Manufacturer.Name,

                    EngineCapacityId = c.EngineCapacityId,
                    EngineCapacity = c.EngineCapacity.Capacity,

                    FuelType = c.FuelType.ToString(),
                    Transmission = c.Transmission.ToString(),

                    Price = c.Price,
                    Year = c.Year,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();

            return Ok(new { statusCode = 200, message = "Cars retrieved successfully", data = cars });
        }

        // GET: api/Cars/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var car = await _context.Cars
                .Include(c => c.Manufacturer)
                .Include(c => c.EngineCapacity)
                .Where(c => c.Id == id)
               .Select(c => new CarDto
               {
                   Id = c.Id,
                   Name = c.Name,

                   ManufacturerId = c.ManufacturerId,
                   Manufacturer = c.Manufacturer.Name,

                   EngineCapacityId = c.EngineCapacityId,
                   EngineCapacity = c.EngineCapacity.Capacity,

                   FuelType = c.FuelType.ToString(),        // ✅ FIXED
                   Transmission = c.Transmission.ToString(),

                   Price = c.Price,
                   Year = c.Year,

                   ImageUrl = c.ImageUrl
               })
                .FirstOrDefaultAsync();

            if (car == null)
                return NotFound();

            return Ok(new { statusCode = 200, message = "Car retrieved successfully", data = car });
        }

        // POST: api/Cars
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CarCreateWithImageDto car)
        {
            car.Name = car.Name.Trim();

            // validate foreign keys
            var manufacturerExists = await _context.Manufacturers.AnyAsync(m => m.Id == car.ManufacturerId);
            var engineExists = await _context.EngineCapacities.AnyAsync(e => e.Id == car.EngineCapacityId);

            if (!manufacturerExists || !engineExists)
                return BadRequest("Invalid ManufacturerId or EngineCapacityId");

            bool exists = await _context.Cars.AnyAsync(c =>
            c.Name.ToLower() == car.Name.ToLower()
            && c.ManufacturerId == car.ManufacturerId
            && c.Year == car.Year);

            if (exists)
            {
                return BadRequest("Car already exists.");

            }
            string imageUrl = "";

            if (car.Image != null)
            {
                imageUrl = await _cloudinaryService
                    .UploadImageAsync(car.Image, "cars");
            }

            var newCar = new Car
            {
                Name = car.Name,
                ManufacturerId = car.ManufacturerId,
                EngineCapacityId = car.EngineCapacityId,

                FuelType = Enum.Parse<FuelType>(car.FuelType),
                Transmission = Enum.Parse<Transmission>(car.Transmission),

                Price = car.Price,
                Year = car.Year,

                ImageUrl = imageUrl   // ✅ FIXED
            };

            _context.Cars.Add(newCar);
            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Car added successfully", data = newCar.Id });
        }

        // PUT: api/Cars/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] CarCreateUpdateDto car)
        {
            var existing = await _context.Cars.FindAsync(id);

            if (existing == null)
                return NotFound();

            car.Name = car.Name.Trim();

            bool exists = await _context.Cars.AnyAsync(c =>
            c.Name.ToLower() == car.Name.ToLower()
            && c.ManufacturerId == car.ManufacturerId
            && c.Year == car.Year
            && c.Id != id);

            if (exists)
            {
                return BadRequest("Car already exists.");
            }
            // update fields
            existing.Name = car.Name;
            existing.ManufacturerId = car.ManufacturerId;
            existing.EngineCapacityId = car.EngineCapacityId;
            existing.FuelType = car.FuelType;
            existing.Transmission = car.Transmission;
            existing.Price = car.Price;
            existing.Year = car.Year;

            if (car.ImageUrl != null)
            {
                existing.ImageUrl = await _cloudinaryService
                    .UploadImageAsync(car.ImageUrl, "cars");
            }

            await _context.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = "Car updated successfully", data = existing.Id });
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

            return Ok(new { statusCode = 200, message = "Car deleted successfully", data = car.Id });
        }

        //upload image for a car
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm]IFormFile file)
        {
            var imageUrl = await _cloudinaryService
                .UploadImageAsync(file, "cars");

            return Ok(imageUrl);
        }
    }
}
