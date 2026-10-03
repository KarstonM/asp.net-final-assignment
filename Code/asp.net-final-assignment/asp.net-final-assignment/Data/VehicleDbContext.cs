using asp.net_final_assignment.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace asp.net_final_assignment.Data
{
    public class VehicleDbContext : IdentityDbContext<AppUser>
    {
        public VehicleDbContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Customer> Customers { get; set; }
    }
}
