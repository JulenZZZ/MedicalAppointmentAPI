namespace AppointmentAPI.DTOs
{
    public class TimeSlotDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string Date { get; set; }        // Formato limpio: "2026-04-10"
        public string StartTime { get; set; }   // Formato limpio: "09:00"
        public string EndTime { get; set; }     // Formato limpio: "09:30"
        public bool IsAvailable { get; set; }
    }
}
