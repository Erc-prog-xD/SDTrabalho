using System.Text.Json;
using ServicoUsuarios.DTO.Admin;
using ServicoUsuarios.Models;

public interface IAdminService
{
    Task<Response<AdminResponseDTO>> VisualizarPerfilAdmin(JsonElement dados);
    Task<Response<object>> AtualizarPerfilAdmin(JsonElement dados);
    Task<Response<object>> DeletarPerfilAdmin(JsonElement dados);
}
