namespace AppointmentAPI.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        //public Appointment? Appointment { get; set; } // Navegación EF
        public decimal Amount { get; set; } // Usar decimal para dinero siempre
        public string Status { get; set; } = "Pending"; // Pending, Paid, Failed, Refunded
        public string PaymentMethod { get; set; } = "Simulated";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; } // Nullable hasta que se confirme

    }
}
