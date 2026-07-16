using AppointmentAPI.Controllers;
using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using System;

namespace AppointmentAPI.Tests
{
    public class TimeSlotsControllerTests
    {
        // Método de ayuda para obtener un DbContext en memoria limpio en cada test
        private AppDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Nombre único para evitar contaminación entre tests
                .Options;

            return new AppDbContext(options);
        }

        [Fact]
        public async Task CreateTimeSlot_ShouldReturnBadRequest_WhenSlotAlreadyExists()
        {
            // 1. ARRANGE (Preparar el escenario)
            var context = GetInMemoryDbContext();

            // Insertamos un médico de prueba
            var doctor = new Doctor { Id = 1, Name = "Dr. House", Specialization = "Diagnostic", Email = "doctor@gmail.com" };
            context.Doctors.Add(doctor);

            // Insertamos un TimeSlot existente idéntico al que intentaremos recrear
            var existingSlot = new TimeSlot
            {
                DoctorId = 1,
                Date = new DateTime(2026, 04, 10),
                StartTime = new TimeSpan(9, 0, 0), // 09:00
                EndTime = new TimeSpan(9, 30, 0),   // 09:30
                IsAvailable = true
            };
            context.TimeSlots.Add(existingSlot);
            await context.SaveChangesAsync();

            var controller = new TimeSlotsController(context);

            // DTO idéntico enviado por el "Admin"
            var duplicateDto = new CreateTimeSlotDto
            {
                DoctorId = 1,
                Date = new DateTime(2026, 04, 10),
                StartTime = "09:00",
                EndTime = "09:30"
            };

            // 2. ACT (Ejecutar la acción)
            var result = await controller.CreateTimeSlot(duplicateDto);

            // 3. ASSERT (Verificar que el resultado es el esperado)
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Validamos que el mensaje de error devuelto sea el correcto
            Assert.Contains("Ya existe una franja horaria idéntica", badRequestResult.Value.ToString());
        }
    }
}
