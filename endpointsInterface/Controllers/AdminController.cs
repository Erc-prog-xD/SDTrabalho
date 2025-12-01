using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EndpointsInterface.DTO.Admins;
using EndpointsInterface.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/Admin")]
    [Authorize(Roles = "Admin")] // Apenas Admin
    public class AdminController : ControllerBase
    {
        private readonly string _usuarioHost;
        private readonly int _usuarioPort;

        public AdminController(IConfiguration config)
        {
            _usuarioHost = config["SERVICO_USUARIOS_HOST"] ?? "localhost";
            _usuarioPort = int.Parse(config["SERVICO_USUARIOS_PORT"] ?? "5005");
        }

        [HttpGet("VisualizarPerfilAdmin")]
        public async Task<IActionResult> VisualizarPerfilAdmin()
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
                    acao = "visualizarperfiladmin",
                    dados = idLogado
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<Response<AdminDTO>>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }

        [HttpDelete("DeletarPerfilAdmin")]
        public async Task<IActionResult> DeletarPerfilAdmin()
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
                    acao = "deletarperfiladmin",
                    dados = idLogado
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

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

        [HttpPut("AtualizarPerfilAdmin")]
        public async Task<IActionResult> AtualizarPerfilAdmin([FromBody] AdminUpdateRequestDTO request)
        {
            try
            {
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (claimId == null)
                    return Unauthorized("Token inválido.");

                int idLogado = int.Parse(claimId);

                var envio = new AdminUpdateEnvioDTO
                {
                    Id = idLogado,
                    Nome = request.Nome,
                    Email = request.Email,
                    Telefone = request.Telefone
                };

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "atualizaradmin",
                    dados = envio
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

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
