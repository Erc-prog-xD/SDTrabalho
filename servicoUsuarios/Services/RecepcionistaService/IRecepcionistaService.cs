using System.Text.Json;
using ServicoUsuarios.DTO.Recepcionista;
using ServicoUsuarios.Models;

public interface IRecepcionistaService
{
    Task<Response<RecepcionistaResponseDTO>> VisualizarPerfilRecepcionista(JsonElement dados);
    Task<Response<object>> AtualizarPerfilRecepcionista(JsonElement dados);
    Task<Response<object>> DeletarPerfilRecepcionista(JsonElement dados);

}
