namespace AppointmentAPI.Entities
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; } // Propiedad de navegación EF
        public DateTime Date { get; set; } // Ejemplo: 2026-04-10
        public TimeSpan StartTime { get; set; } // Ejemplo: 09:00:00
        public TimeSpan EndTime { get; set; } // Ejemplo: 09:30:00
        public bool IsAvailable { get; set; } = true; // Controla si está libre puntualmente
    }
}
