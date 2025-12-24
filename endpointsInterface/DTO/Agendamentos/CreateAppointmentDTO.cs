namespace endpointsInterface.DTO.Agendamentos;

public class CreateAppointmentDTO
{
    public int PatientId { get; set; } = default!;
    public int DoctorId { get; set; } = default!;
    public string Specialty { get; set; } = default!;
    public DateTime Datetime { get; set; } = default!;
}