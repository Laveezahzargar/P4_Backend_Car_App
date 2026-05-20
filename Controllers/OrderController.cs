using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using P4_Backend_Car_App.Types;
using Serilog;

namespace P4_Backend_Car_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrderController(AppDbContext context)
        {
            _context=context;
        }
        [HttpPost("CreateOrder/{carId}")]
        public async Task<IActionResult> CreateOrder(int carId)
        {
            var userIdClaim = User.FindFirst("id")?.Value;

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var car = await _context.Cars.FindAsync(carId);

            if (car == null)
                return NotFound("Car not found");
            Log.Information($"UserId: {userId}, CarId: {carId}");
            var order = new Order
            {
                UserId = userId,
                CarId = carId,
                Price = car.Price
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            Log.Information("Order saved!");

            return Ok(new { statusCode = 200, message = "Order Created Sucessfully.", data = order });
        }
        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Car)
                .ToListAsync();

            return Ok(new {StatusCode=200,message="Orders retrieved sucessfully." ,data=orders});
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null) return NotFound();

            order.Status = status;

            await _context.SaveChangesAsync();

            return Ok(new { StatusCode = 200, message = "OrderStatus updated sucessfully.", data = order });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null) return NotFound();

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
