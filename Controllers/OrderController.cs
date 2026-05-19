using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P4_Backend_Car_App.Data;
using P4_Backend_Car_App.Models;
using P4_Backend_Car_App.Types;

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
