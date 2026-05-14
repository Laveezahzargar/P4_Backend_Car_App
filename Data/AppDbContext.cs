using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;

using P4_Backend_Car_App.Models;

namespace P4_Backend_Car_App.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<EngineCapacity> EngineCapacities { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    }
}
