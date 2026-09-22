using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        [HttpGet]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetAllAvailableDoctors([FromQuery] int? doctorId)
        {
            // Consulta en Entity Framework que devuelva todos los Doctprs con IsActive == true
            var doctors = await _context.Doctors
                .Where(t => t.IsActive)
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("available")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAllAvailableTimeSlots([FromQuery] string? date)
        {
            // Consulta en Entity Framework que devuelva todos los TimeSlots con IsBooked == false
            var slots = await _context.TimeSlots
                .Include(t => t.Doctor)
                .Where(t => t.IsAvailable)
                .ToListAsync(); 

            return Ok(slots);
        }

        [HttpGet("{doctorId}/available-timeslots")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date)
        {
            // 1. Iniciar la consulta filtrando por Médico y que esté Disponible
            var query = _context.TimeSlots
                .Include(ts => ts.Doctor) // Importante incluir la relación para que ts.Doctor.Name no dé NullReferenceException
                .Where(ts => ts.DoctorId == doctorId && ts.IsAvailable);

            // 2. Filtrar por fecha SOLO SI el parámetro date fue enviado
            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(ts => ts.Date.Date == targetDate);
            }

            var slots = await query
                .OrderBy(ts => ts.StartTime)
                .ToListAsync();

            // 3. Mapear las entidades al DTO de salida
            var result = slots.Select(ts => new TimeSlotDto
            {
                Id = ts.Id,
                DoctorId = ts.DoctorId,
                DoctorName = ts.Doctor.Name,
                Specialization = ts.Doctor.Specialization,
                Date = ts.Date.ToString("yyyy-MM-dd"),
                StartTime = ts.StartTime.ToString(@"hh\:mm"), // Formatea el TimeSpan a "09:00"
                EndTime = ts.EndTime.ToString(@"hh\:mm"),
                IsAvailable = ts.IsAvailable
            }).ToList();

            return Ok(result);
        }
    }
}