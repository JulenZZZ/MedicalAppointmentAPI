using AppointmentAPI.Controllers;
using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AppointmentAPI.Tests
{
    public class AppointmentsControllerTests
    {
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options);
        }

        // Método auxiliar para simular el contexto de un usuario logueado en el controlador
        private void MockUserInController(ControllerBase controller, string userId, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public async Task CancelAppointment_ShouldReturnForbidden_WhenPatientAttemptsToCancelOthersAppointment()
        {
            // 1. ARRANGE
            var context = GetInMemoryDbContext();

            // Creamos un TimeSlot y una cita que pertenece al paciente ID: 9999 (Víctima)
            var slot = new TimeSlot
            {
                Id = 10,
                DoctorId = 1,
                Date = DateTime.Today,
                IsAvailable = false
            };
            var appointment = new Appointment
            {
                Id = 1,
                TimeSlotId = 10,
                PatientId = 9999, // <--- ID del dueño real de la cita
                Status = "Scheduled"
            };

            context.TimeSlots.Add(slot);
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            var controller = new AppointmentsController(context);

            // Simulamos que quien hace la petición es "Panchito" (ID: 2004, Rol: Patient)
            MockUserInController(controller, "2004", "Patient");

            // 2. ACT
            var result = await controller.CancelAppointment(1); // Intenta cancelar la cita ID 1

            // 3. ASSERT
            // Debe retornar un ForbidResult (403 Forbidden)
            Assert.IsType<ForbidResult>(result);
        }
        

        [Fact]
        public async Task CreateAppointment_ShouldBookSlotAndMarkUnavailable_WhenSlotIsAvailable()
        {
            var context = GetInMemoryDbContext();
            context.Doctors.Add(new Doctor { Id = 1, Name = "Dr. House", Specialization = "General", Email = "doc@yourmed.com" });
            context.Users.Add(new User { Id = 500, Name = "Patient 500", Email = "p500@yourmed.com", Role = Roles.Patient, PasswordHash = "hash" });

            var slot = new TimeSlot
            {
                Id = 20,
                DoctorId = 1,
                Date = DateTime.UtcNow.AddDays(1),
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                IsAvailable = true
            };
            context.TimeSlots.Add(slot);
            await context.SaveChangesAsync();

            var controller = new AppointmentsController(context);
            MockUserInController(controller, "500", "Patient");

            var result = await controller.CreateAppointment(new CreateAppointmentDto { TimeSlotId = 20 });

            Assert.IsType<CreatedAtActionResult>(result);
            var updatedSlot = await context.TimeSlots.FindAsync(20);
            Assert.False(updatedSlot!.IsAvailable);
        }

        [Fact]
        public async Task CreateAppointment_ShouldReturnBadRequest_WhenSlotIsAlreadyBooked()
        {
            var context = GetInMemoryDbContext();
            context.Doctors.Add(new Doctor { Id = 1, Name = "Dr. House", Specialization = "General", Email = "doc@yourmed.com" });

            var occupiedSlot = new TimeSlot
            {
                Id = 21,
                DoctorId = 1,
                Date = DateTime.UtcNow.AddDays(1),
                IsAvailable = false
            };
            context.TimeSlots.Add(occupiedSlot);
            await context.SaveChangesAsync();

            var controller = new AppointmentsController(context);
            MockUserInController(controller, "501", "Patient");

            var result = await controller.CreateAppointment(new CreateAppointmentDto { TimeSlotId = 21 });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Appointments);
        }

        [Fact]
        public async Task GetMyAppointments_ShouldReturnOnlyAuthenticatedPatientAppointments()
        {
            var context = GetInMemoryDbContext();
            context.Doctors.Add(new Doctor { Id = 1, Name = "Dr. House", Specialization = "General", Email = "doc@yourmed.com" });
            context.Users.AddRange(
                new User { Id = 2004, Name = "Patient 2004", Email = "p2004@yourmed.com", Role = Roles.Patient, PasswordHash = "hash" },
                new User { Id = 9999, Name = "Patient 9999", Email = "p9999@yourmed.com", Role = Roles.Patient, PasswordHash = "hash" }
            );

            var slot1 = new TimeSlot { Id = 40, DoctorId = 1, Date = DateTime.UtcNow.AddDays(1), IsAvailable = false };
            var slot2 = new TimeSlot { Id = 41, DoctorId = 1, Date = DateTime.UtcNow.AddDays(2), IsAvailable = false };

            context.TimeSlots.AddRange(slot1, slot2);
            context.Appointments.AddRange(
                new Appointment { Id = 10, TimeSlotId = 40, DoctorId = 1, PatientId = 2004, Status = "Scheduled" },
                new Appointment { Id = 11, TimeSlotId = 41, DoctorId = 1, PatientId = 9999, Status = "Scheduled" }
            );
            await context.SaveChangesAsync();

            var controller = new AppointmentsController(context);
            MockUserInController(controller, "2004", "Patient");

            var result = await controller.GetPatientAppointments();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<IEnumerable<AppointmentResponseDto>>(okResult.Value);
            Assert.Single(appointments);
        }

        [Fact]
        public async Task GetAppointmentById_ShouldReturnForbidden_WhenPatientQueriesAnotherPatientsAppointment()
        {
            var context = GetInMemoryDbContext();
            context.Doctors.Add(new Doctor { Id = 1, Name = "Dr. House", Specialization = "General", Email = "doc@yourmed.com" });
            context.Users.Add(new User { Id = 9999, Name = "Victim", Email = "victim@yourmed.com", Role = Roles.Patient, PasswordHash = "hash" });

            var slot = new TimeSlot { Id = 50, DoctorId = 1, Date = DateTime.UtcNow.AddDays(1), IsAvailable = false };
            var appointment = new Appointment { Id = 77, TimeSlotId = 50, DoctorId = 1, PatientId = 9999, Status = "Scheduled" };

            context.TimeSlots.Add(slot);
            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            var controller = new AppointmentsController(context);
            MockUserInController(controller, "2004", "Patient");

            var result = await controller.GetAppointmentById(77);

            Assert.IsType<ForbidResult>(result);
        }
    }
}
