using endpointsInterface.DTO.Agendamentos;
using Microsoft.AspNetCore.Mvc;
using Scheduling.Grpc;

namespace endpointsInterface.Controllers;

[ApiController]
[Route("api/scheduling")]
public class SchedulingController : ControllerBase
{
    private readonly SchedulingService.SchedulingServiceClient _grpcClient;

    public SchedulingController(SchedulingService.SchedulingServiceClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO dto)
    {
        var request = new CreateAppointmentRequest
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Specialty = dto.Specialty,
            Datetime = dto.Datetime
        };

        var response = await _grpcClient.CreateAppointmentAsync(request);

        return Ok(response.Appointment);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(string patientId)
    {
        var request = new ListAppointmentsRequest
        {
            UserId = patientId,
            IsDoctor = false
        };

        var response = await _grpcClient.ListAppointmentsAsync(request);
        return Ok(response.Appointments);
    }

    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetByDoctor(string doctorId)
    {
        var request = new ListAppointmentsRequest
        {
            UserId = doctorId,
            IsDoctor = true
        };

        var response = await _grpcClient.ListAppointmentsAsync(request);
        return Ok(response.Appointments);
    }

    //// PUT: /api/scheduling/status
    //[HttpPut("status")]
    //public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
    //{
    //    // converte string para enum gRPC
    //    if (!Enum.TryParse<AppointmentStatus>(dto.NewStatus, out var status))
    //    {
    //        return BadRequest("Status inválido. Use valores como STATUS_CONFIRMED, STATUS_CANCELLED, etc.");
    //    }

    //    var request = new UpdateStatusRequest
    //    {
    //        AppointmentId = dto.AppointmentId,
    //        NewStatus = status
    //    };

    //    var response = await _grpcClient.UpdateStatusAsync(request);
    //    return Ok(response.Appointment);
    //}
}
