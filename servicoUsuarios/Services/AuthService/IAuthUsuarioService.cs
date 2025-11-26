using System.Text.Json;
using ServicoUsuarios.Models;

public interface IAuthUsuarioService
{
    Task<Response<string>> Registrar(JsonElement dados);
    Task<Response<string>> Login(JsonElement dados);
}
