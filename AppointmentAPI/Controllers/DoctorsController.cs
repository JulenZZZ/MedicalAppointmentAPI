using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppointmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // <-- CERRADO BAJO LLAVE: Solo entra si el JWT tiene el Claim de "Admin"
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Médico {doctor.Name} creado exitosamente." });
        }
        [HttpGet("{doctorId}/available-slots")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
        {
            // 1. Filtrar los slots por Médico, Fecha exacta y que estén DISPONIBLES
            var slots = await _context.TimeSlots
                .Where(ts => ts.DoctorId == doctorId &&
                             ts.Date == date.Date &&
                             ts.IsAvailable == true)
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            // 2. Mapear las entidades al DTO de salida
            var result = slots.Select(ts => new TimeSlotDto
            {
                Id = ts.Id,
                DoctorId = ts.DoctorId,
                Date = ts.Date.ToString("yyyy-MM-dd"),
                StartTime = ts.StartTime.ToString(@"hh\:mm"), // Formatea el TimeSpan a "09:00"
                EndTime = ts.EndTime.ToString(@"hh\:mm"),
                IsAvailable = ts.IsAvailable
            }).ToList();

            return Ok(result);
        }
    }
}