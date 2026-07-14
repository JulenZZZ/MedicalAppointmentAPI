using AppointmentAPI.Data;
using AppointmentAPI.DTOs;
using AppointmentAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppointmentAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class AppointmentsController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDto dto)
        {
            // 1. Obtener el ID del Paciente autenticado desde el Token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int patientId))
            {
                return Unauthorized(new { message = "Token inválido o usuario no identificado." });
            }
            // 2. Buscar el TimeSlot y verificar que exista y esté libre
            var timeSlot = await _context.TimeSlots.FirstOrDefaultAsync(ts => ts.Id == dto.TimeSlotId);
            if (timeSlot == null)
            {
                return NotFound(new { message = "La franja horaria seleccionada no existe." });
            }

            if (!timeSlot.IsAvailable)
            {
                return BadRequest(new { message = "Esta franja horaria ya ha sido reservada por otro paciente." });
            }
            // --- INICIO DE LA TRANSACCIÓN LÓGICA ---
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Modificar el estado de disponibilidad del TimeSlot a falso
                timeSlot.IsAvailable = false;

                // 4. Crear la Cita (Appointment)
                var appointment = new Appointment
                {
                    TimeSlotId = timeSlot.Id,
                    DoctorId = timeSlot.DoctorId,
                    PatientId = patientId,
                    CreatedAt = DateTime.UtcNow,
                    Status = "Scheduled",
                    PaymentStatus = "Paid" // En V1 asumimos que el pago pasa directo
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync(); // Guardamos para obtener el Id de la cita generado por la BD

                // 5. Crear el registro del Pago Simulado (Payment) asociado a la cita
                var payment = new Payment
                {
                    AppointmentId = appointment.Id,
                    Amount = 50.00m, // Tarifa fija simulada (ejemplo: $50.00)
                    Status = "Paid",
                    PaymentMethod = "Simulated Card",
                    CreatedAt = DateTime.UtcNow,
                    PaidAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Confirmar todos los cambios en la base de datos de manera atómica
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Cita reservada y pago procesado con éxito de forma simulada.",
                    appointmentId = appointment.Id,
                    paymentId = payment.Id
                });
            }
            catch (Exception)
            {
                // Si algo falla a mitad de camino, revertimos todo para no dejar datos corruptos
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Ocurrió un error al procesar la reserva de la cita." });
            }
        }
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin,Patient")] // Ambos pueden cancelar, pero con diferentes reglas
        public async Task<IActionResult> CancelAppointment(int id)
        {
            // 1. Obtener datos del usuario autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int currentUserId))
            {
                return Unauthorized(new { message = "Token inválido o usuario no identificado." });
            }

            // 2. Buscar la cita junto con su TimeSlot y su Pago
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound(new { message = "La cita especificada no existe." });
            }

            // 3. VALIDACIÓN DE SEGURIDAD: 
            // Si es un Paciente, verificar que la cita realmente le pertenezca
            if (userRoleClaim == "Patient" && appointment.PatientId != currentUserId)
            {
                return Forbid(); // 403 Forbidden - No puedes cancelar citas de otros
            }

            if (appointment.Status == "Cancelled")
            {
                return BadRequest(new { message = "Esta cita ya se encuentra cancelada." });
            }

            // --- TRANSACCIÓN LÓGICA DE CANCELACIÓN ---
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // A. Cambiar estado de la cita
                appointment.Status = "Cancelled";

                // B. Liberar el horario para que vuelva a estar disponible
                if (appointment.TimeSlot != null)
                {
                    appointment.TimeSlot.IsAvailable = true;
                }

                // C. Buscar el pago asociado y marcarlo como reembolsado
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.AppointmentId == appointment.Id);
                if (payment != null)
                {
                    payment.Status = "Refunded";
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new { message = "Cita cancelada con éxito. El horario ha sido liberado y el pago reembolsado." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Ocurrió un error al procesar la cancelación." });
            }
        }
    }
}
