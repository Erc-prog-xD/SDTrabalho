using System.Text.Json;
using ServicoUsuarios.DTO.Pacientes;
using ServicoUsuarios.Models;

public interface IPacienteService
{
    Task<Response<PacienteResponseDTO>> VisualizarPerfil(JsonElement dados);
    Task<Response<object>> AtualizarPerfil(JsonElement dados);

}
