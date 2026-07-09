namespace AppointmentAPI.Entities
{
    public class Doctor
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Specialization { get; set; }
        public required string Email { get; set; }
        public bool IsActive {  get; set; } = true;
    }
}
