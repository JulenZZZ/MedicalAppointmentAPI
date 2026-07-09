namespace AppointmentAPI.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        public int TimeSlotId { get; set; }
        public TimeSlot? TimeSlot { get; set; }
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public int PatientId { get; set; } // El Id del usuario con rol Patient
        public User? Patient { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Estados de la cita: "Scheduled", "Cancelled", "Completed"
        public string Status { get; set; } = "Scheduled";
        // Estados de pago simulados para V1: "Pending", "Paid", "Refunded"
        public string PaymentStatus { get; set; } = "Paid";
    }
}
