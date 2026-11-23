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
    [Authorize]
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
            var doctors = await _context.Doctors
                .Where(t => t.IsActive)
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("available")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetAllAvailableTimeSlots([FromQuery] string? date)
        {
            var today = DateTime.UtcNow.Date;
            var currentTime = DateTime.UtcNow.TimeOfDay;

            var query = _context.TimeSlots
                .Include(t => t.Doctor)
                .Where(t => t.IsAvailable &&
                            (t.Date.Date > today || (t.Date.Date == today && t.StartTime > currentTime)));

            if (!string.IsNullOrWhiteSpace(date) && DateTime.TryParse(date, out var parsedDate))
            {
                var targetDate = parsedDate.Date;
                query = query.Where(t => t.Date.Date == targetDate);
            }

            var slots = await query
                .OrderBy(t => t.Date)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            var result = slots.Select(ts => new TimeSlotDto
            {
                Id = ts.Id,
                DoctorId = ts.DoctorId,
                DoctorName = ts.Doctor.Name,
                Specialization = ts.Doctor.Specialization,
                Date = ts.Date.ToString("yyyy-MM-dd"),
                StartTime = ts.StartTime.ToString(@"hh\:mm"),
                EndTime = ts.EndTime.ToString(@"hh\:mm"),
                IsAvailable = ts.IsAvailable
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{doctorId}/available-timeslots")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date)
        {
            var today = DateTime.UtcNow.Date;
            var currentTime = DateTime.UtcNow.TimeOfDay;

            var query = _context.TimeSlots
                .Include(ts => ts.Doctor)
                .Where(ts => ts.DoctorId == doctorId &&
                             ts.IsAvailable &&
                             (ts.Date.Date > today || (ts.Date.Date == today && ts.StartTime > currentTime)));

            if (date.HasValue)
            {
                var targetDate = date.Value.Date;
                query = query.Where(ts => ts.Date.Date == targetDate);
            }

            var slots = await query
                .OrderBy(ts => ts.Date)
                .ThenBy(ts => ts.StartTime)
                .ToListAsync();

            var result = slots.Select(ts => new TimeSlotDto
            {
                Id = ts.Id,
                DoctorId = ts.DoctorId,
                DoctorName = ts.Doctor.Name,
                Specialization = ts.Doctor.Specialization,
                Date = ts.Date.ToString("yyyy-MM-dd"),
                StartTime = ts.StartTime.ToString(@"hh\:mm"),
                EndTime = ts.EndTime.ToString(@"hh\:mm"),
                IsAvailable = ts.IsAvailable
            }).ToList();

            return Ok(result);
        }
    }
}