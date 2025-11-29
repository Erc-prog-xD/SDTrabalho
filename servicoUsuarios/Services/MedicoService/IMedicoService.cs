using System.Text.Json;
using ServicoUsuarios.DTO.Medico;
using ServicoUsuarios.Models;

public interface IMedicoService
{
    Task<Response<MedicoResponseDTO>> VisualizarPerfilMedico(JsonElement dados);
    Task<Response<object>> AtualizarPerfilMedico(JsonElement dados);
    Task<Response<object>> DeletarPerfilMedico(JsonElement dados);

}
