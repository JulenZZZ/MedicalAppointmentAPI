namespace AppointmentAPI.DTOs
{
    public class CreateAppointmentDto
    {
        public int TimeSlotId { get; set; }
        public string PaymentMethod { get; set; } = "SimulatedCreditCard";
        public decimal Amount { get; set; } = 50.00m; // Monto por defecto de la consulta
    }
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public int TimeSlotId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public string Status { get; set; } = "Confirmed";
        public string PaymentStatus { get; set; } = "Paid";
        public string TransactionId { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
    }
}
