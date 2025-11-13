using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly string _usuarioHost;
        private readonly int _usuarioPort;

        public UsuarioController(IConfiguration config)
        {
            _usuarioHost = config["SERVICO_USUARIOS_HOST"] ?? "localhost";
            _usuarioPort = int.Parse(config["SERVICO_USUARIOS_PORT"] ?? "5005");

        }

        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);

                using NetworkStream stream = client.GetStream();

                // Monta o envelope para o serviço de usuário
                var envelope = new
                {
                    acao = "login",
                    dados = request
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                // Recebe resposta
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<UsuarioResponse>(responseJson);

                if (response == null || !response.Sucesso)
                    return Unauthorized(new { sucesso = false, mensagem = response?.Mensagem ?? "Erro no login." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioRegisterRequest request)
        {
            try
            {
                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);

                using NetworkStream stream = client.GetStream();

                // Monta o JSON que o serviço de usuários espera
                var envelope = new
                {
                    acao = "registrar",
                    dados = request
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                // Aguarda resposta do serviço
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<UsuarioResponse>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }
    }

    public class LoginRequest
    {
        public required string Cpf { get; set; }
        public required string Senha { get; set; } 
    }
    public class UsuarioRegisterRequest
    {
        public required string Cpf { get; set; }
        public required  string Nome { get; set; }
        public required string Senha { get; set; } 
        public required UsertypeEnum Role { get; set; }

    }

    public class UsuarioResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}
