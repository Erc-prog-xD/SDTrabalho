using endpointsInterface.DTO.Agendamentos;
using EndpointsInterface.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scheduling.Grpc;

namespace endpointsInterface.Controllers;

[ApiController]
[Route("api/scheduling")]
[Authorize(Roles = "Admin, Recepcionista, Medico, Paciente")]
public class SchedulingController : ControllerBase
{
    private readonly SchedulingService.SchedulingServiceClient _grpcClient;

    public SchedulingController(SchedulingService.SchedulingServiceClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    [HttpPost]
    [Authorize(Roles = "Admin, Recepcionista, Medico")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO dto)
    {
        var request = new CreateAppointmentRequest
        {
            PatientId = dto.PatientId,
            DoctorId = dto.DoctorId,
            Specialty = dto.Specialty,
            Datetime = dto.Datetime.ToString("yyyy-MM-dd'T'HH:mm:ss")
        };

        var response = await _grpcClient.CreateAppointmentAsync(request);

        return Ok(response);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var request = new ListAppointmentsRequest
        {
            UserId = patientId,
            IsDoctor = false
        };

        var response = await _grpcClient.ListAppointmentsAsync(request);
        return Ok(response);
    }

    [HttpGet("doctor/{doctorId}")]
    [Authorize(Roles = "Admin, Recepcionista, Medico")]
    public async Task<IActionResult> GetByDoctor(int doctorId)
    {
        var request = new ListAppointmentsRequest
        {
            UserId = doctorId,
            IsDoctor = true
        };

        var response = await _grpcClient.ListAppointmentsAsync(request);
        return Ok(response);
    }

    [HttpPatch("{appointmentId}/status/{newStatus}")]
    [Authorize(Roles = "Admin, Recepcionista, Medico")]
    public async Task<IActionResult> UpdateStatus(int appointmentId, int newStatus)
    {
        if (!Enum.IsDefined(typeof(AppointmentStatus), newStatus))
            return BadRequest(
                new Response<object>
                {
                    Status = false,
                    Dados = null,
                    Mensage = "Status inválido"
                }
            );

        var request = new UpdateStatusRequest
        {
            AppointmentId = appointmentId,
            NewStatus = (AppointmentStatus)newStatus
        };

        var response = await _grpcClient.UpdateStatusAsync(request);
        return Ok(response);
    }

    [HttpDelete("{appointmentId}")]
    [Authorize(Roles = "Admin, Recepcionista, Medico")]
    public async Task<IActionResult> DeleteAppointment(int appointmentId)
    {
        var request = new DeleteAppointmentRequest
        {
            AppointmentId = appointmentId,
        };

        var response = await _grpcClient.DeleteAppointmentAsync(request);
        return Ok(response);
    }
}
