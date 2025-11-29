using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EndpointsInterface.DTO.Recepcionistas;
using EndpointsInterface.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/Recepcionista")]
    [Authorize(Roles = "Recepcionista")] // Apenas Paciente ou Admin
    public class RecepcionistaController : ControllerBase
    {
       private readonly string _usuarioHost;
        private readonly int _usuarioPort;

        public RecepcionistaController(IConfiguration config)
        {
            _usuarioHost = config["SERVICO_USUARIOS_HOST"] ?? "localhost";
            _usuarioPort = int.Parse(config["SERVICO_USUARIOS_PORT"] ?? "5005");

        }

        [HttpGet("VisualizarPerfilRecepcionista")]
        public async Task<IActionResult> VisualizarPerfilRecepcionista()
        {
              try
            {
               var claimId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (claimId == null)
                    return Unauthorized("Token inválido.");

                int idLogado = int.Parse(claimId);

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "visualizarperfilrecepcionista",
                    dados = idLogado
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                 // Aguarda resposta do serviço
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<Response<RecepcionistaDTO>>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }
        [HttpDelete("DeletarPerfilRecepcionista")]
        public async Task<IActionResult> DeletarPerfilRecepcionista()
        {
              try
            {
               var claimId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (claimId == null)
                    return Unauthorized("Token inválido.");

                int idLogado = int.Parse(claimId);

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "deletarperfilrecepcionista",
                    dados = idLogado
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

        [HttpPut("AtualizarPerfilRecepcionista")]
        public async Task<IActionResult> AtualizarPerfilRecepcionista([FromBody] RecepcionistaUpdateRequestDTO request)
        {
            try
            {
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (claimId == null)
                    return Unauthorized("Token inválido.");

                int idLogado = int.Parse(claimId);
                
                var envio = new  RecepcionistaUpdateEnvioDTO{
                    Id = idLogado,
                    Nome = request.Nome,
                    Email = request.Email,
                    Telefone = request.Telefone,
                    Turno = request.Turno                    
                };

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "atualizarrecepcionista",
                    dados = envio
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
