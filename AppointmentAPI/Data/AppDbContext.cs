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
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Payment> Payments { get; set; }

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
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            // 2. SOLUCIÓN AL ERROR DE CASCADA: Decirle a EF que si se borra un Doctor o un TimeSlot, 
            // NO borre la cita automáticamente en cascada (lo manejaremos de forma controlada si hace falta).
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Evita el camino cíclico

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.TimeSlot)
                .WithMany()
                .HasForeignKey(a => a.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict); // Evita el camino cíclico
        }
    }
}
