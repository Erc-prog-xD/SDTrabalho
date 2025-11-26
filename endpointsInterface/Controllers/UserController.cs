using EndpointsInterface.DTO;
using EndpointsInterface.Models;
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

        
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
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

                var response = JsonSerializer.Deserialize<Response<string>>(responseJson);

                if (response == null || !response.Status)
                    return Unauthorized(new { sucesso = false, mensagem = response?.Mensage ?? "Erro no login." });

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> Registrar([FromBody] UsuarioRegisterRequestDTO request)
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

                var response = JsonSerializer.Deserialize<Response<string>>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }
    }

}
