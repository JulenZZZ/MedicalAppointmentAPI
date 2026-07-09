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
    [Authorize(Roles = Roles.Admin)]
    public class TimeSlotsController : Controller
    {
        private readonly AppDbContext _context;
        public TimeSlotsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimeSlot([FromBody] CreateTimeSlotDto dto)
        {
            // 1. Validar que el médico exista
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId);
            if (!doctorExists)
            {
                return NotFound(new { message = "El médico especificado no existe." });
            }
            // 2. Parsear las horas recibidas en formato string a TimeSpan
            if (!TimeSpan.TryParse(dto.StartTime, out TimeSpan startTime) ||
                !TimeSpan.TryParse(dto.EndTime, out TimeSpan endTime))
            {
                return BadRequest(new { message = "El formato de StartTime o EndTime es inválido (Ej correcto: '09:00')." });
            }

            // --- 3. NUEVA VALIDACIÓN: Evitar cruces de horarios exactos ---
            var slotExists = await _context.TimeSlots.AnyAsync(ts =>
                ts.DoctorId == dto.DoctorId &&
                ts.Date == dto.Date.Date &&
                ts.StartTime == startTime &&
                ts.EndTime == endTime
            );

            if (slotExists)
            {
                return BadRequest(new { message = "Ya existe una franja horaria idéntica registrada para este médico en este mismo día y hora." });
            }
            // -------------------------------------------------------------

            // 4. Crear e insertar la franja horaria
            var timeSlot = new TimeSlot
            {
                DoctorId = dto.DoctorId,
                Date = dto.Date.Date, // Guardamos solo la parte de la fecha
                StartTime = startTime,
                EndTime = endTime,
                IsAvailable = true // Por defecto inicia libre
            };

            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Franja horaria generada exitosamente." });
        }

    }
}
