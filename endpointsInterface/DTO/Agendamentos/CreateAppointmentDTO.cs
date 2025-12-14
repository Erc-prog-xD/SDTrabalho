namespace endpointsInterface.DTO.Agendamentos;

public class CreateAppointmentDTO
{
    public string PatientId { get; set; } = default!;
    public string DoctorId { get; set; } = default!;
    public string Specialty { get; set; } = default!;
    public string DatetimeIso { get; set; } = default!;
}