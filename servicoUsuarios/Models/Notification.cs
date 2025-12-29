namespace ServicoUsuarios.Models
{
    public class Notification
    {
        public long Id { get; set; }                 // opcional, se tiver tabela
        public long AppointmentId { get; set; }
        public long PatientId { get; set; }
        public long DoctorId { get; set; }
        public string Status { get; set; } = "";     // novo status (ex: STATUS_CONFIRMED)
        public string Message { get; set; } = "";    // texto livre
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // se você for persistir e depois publicar:
        public bool Published { get; set; } = false;
        public DateTime? DeletionDate {get; set;} = null;

    }
}
