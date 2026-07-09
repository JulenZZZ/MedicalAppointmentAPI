using AppointmentAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 999,
                    Name = "Administrador Principal",
                    Email = "admin@appointment.com",
                    PasswordHash = "AQAAAAIAAYagAAAAEJgr8JPjKlgLGanEhpsPACWm5CmX+2szLSoPlsNXf4MWw0MSLcNButQq3l33+lFqfQ==", // El hash correspondiente al password que uses
                    Role = Roles.Admin // <-- Aquí forzamos que sea Admin explícitamente
                }
            );
        }
    }
}
