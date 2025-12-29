namespace NotificationService.models
{
public class Notification
{
        public long Id { get; set; }  // <<< importante para marcar Published
        public long AppointmentId { get; set; }
        public long PatientId { get; set; }
        public long DoctorId { get; set; }
        public string Status { get; set; } = "";
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Published { get; set; } = false;
        public DateTime? DeletionDate { get; set; }
}

}