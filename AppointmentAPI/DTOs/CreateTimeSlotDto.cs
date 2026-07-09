namespace AppointmentAPI.DTOs
{
    public class CreateTimeSlotDto
    {
        public int DoctorId { get; set; }
        public DateTime Date { get; set; } // Ejemplo: 2026-04-10
        public string StartTime { get; set; } // Lo recibimos como string "09:00" para procesarlo fácil
        public string EndTime { get; set; }   // Lo recibimos como string "09:30"
    }
}
