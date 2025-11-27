using System.Text.Json;
using ServicoUsuarios.Models;

public interface IAuthUsuarioService
{
    Task<Response<object>> Registrar(JsonElement dados);
    Task<Response<object>> Login(JsonElement dados);
}
