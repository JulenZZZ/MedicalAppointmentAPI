using AppointmentAPI.Controllers;
using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentAPI.Tests
{
    public class DoctorsControllerTests
    {
        [Fact]
        public async Task GetAllAvailableTimeSlots_ExcludesExpiredSlots_ReturnsOnlyFutureSlots()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var doctor = new Doctor
            {
                Id = 1,
                Name = "Dr. Emily Carter",
                Specialization = "Cardiology",
                Email = "emily@yourmed.com",
                IsActive = true
            };

            var expiredSlot = new TimeSlot
            {
                Id = 10,
                DoctorId = doctor.Id,
                Doctor = doctor,
                Date = DateTime.UtcNow.Date.AddDays(-1), // Ayer (caducado)
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(9, 30, 0),
                IsAvailable = true
            };

            var futureSlot = new TimeSlot
            {
                Id = 20,
                DoctorId = doctor.Id,
                Doctor = doctor,
                Date = DateTime.UtcNow.Date.AddDays(1), // Mañana (vigente)
                StartTime = new TimeSpan(10, 30, 0),
                EndTime = new TimeSpan(11, 0, 0),
                IsAvailable = true
            };

            context.Doctors.Add(doctor);
            context.TimeSlots.AddRange(expiredSlot, futureSlot);
            await context.SaveChangesAsync();

            var controller = new DoctorsController(context);

            // Act
            var actionResult = await controller.GetAllAvailableTimeSlots(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var returnedSlots = Assert.IsAssignableFrom<IEnumerable<TimeSlotDto>>(okResult.Value).ToList();

            Assert.Single(returnedSlots);
            Assert.Equal(20, returnedSlots[0].Id);
        }

        [Fact]
        public async Task SeedAsync_DeactivatesExpiredFreeSlots_AndGeneratesFutureSlots()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new AppDbContext(options);

            var doctor = new Doctor
            {
                Id = 1,
                Name = "Dr. Marcus Vance",
                Specialization = "Dermatology",
                Email = "marcus@yourmed.com",
                IsActive = true
            };

            var expiredFreeSlot = new TimeSlot
            {
                Id = 100,
                DoctorId = doctor.Id,
                Date = DateTime.UtcNow.Date.AddDays(-2),
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(9, 30, 0),
                IsAvailable = true // Libre pero caducado
            };

            context.Doctors.Add(doctor);
            context.TimeSlots.Add(expiredFreeSlot);
            await context.SaveChangesAsync();

            // Act
            await DbSeeder.SeedAsync(context);

            // Assert
            var updatedExpiredSlot = await context.TimeSlots.FindAsync(100);
            Assert.NotNull(updatedExpiredSlot);
            Assert.False(updatedExpiredSlot.IsAvailable); // Debe quedar desactivado

            var hasFutureSlots = await context.TimeSlots.AnyAsync(ts =>
                ts.DoctorId == doctor.Id &&
                ts.Date.Date > DateTime.UtcNow.Date &&
                ts.IsAvailable);
            Assert.True(hasFutureSlots); // Debe haber generado turnos futuros
        }
    }
}
