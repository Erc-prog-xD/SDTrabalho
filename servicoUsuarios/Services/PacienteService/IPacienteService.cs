using System.Text.Json;

using ServicoUsuarios.DTO.Paciente;
using ServicoUsuarios.Models;

public interface IPacienteService
{
    Task<Response<PacienteResponseDTO>> VisualizarPerfilPaciente(JsonElement dados);
    Task<Response<object>> AtualizarPerfilPaciente(JsonElement dados);
    Task<Response<object>> DeletarPerfilPaciente(JsonElement dados);

}
