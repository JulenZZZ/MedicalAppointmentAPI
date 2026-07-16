using AppointmentAPI.Controllers;
using AppointmentAPI.Data;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    }
}
