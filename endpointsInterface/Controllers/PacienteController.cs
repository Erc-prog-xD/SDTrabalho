using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EndpointsInterface.DTO.Pacientes;
using EndpointsInterface.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/paciente")]
    [Authorize(Roles = "Paciente,Admin")] // Apenas Paciente ou Admin
    public class PacienteController : ControllerBase
    {
       private readonly string _usuarioHost;
        private readonly int _usuarioPort;

        public PacienteController(IConfiguration config)
        {
            _usuarioHost = config["SERVICO_USUARIOS_HOST"] ?? "localhost";
            _usuarioPort = int.Parse(config["SERVICO_USUARIOS_PORT"] ?? "5005");

        }

        [HttpGet("visualizarPerfil")]
        public async Task<IActionResult> VisualizarPerfil()
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
                    acao = "visualizarperfil",
                    dados = idLogado
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                 // Aguarda resposta do serviço
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<Response<PacienteDTO>>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }

        [HttpPut("atualizarPerfil")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] PacienteUpdateRequestDTO request)
        {
            try
            {
                var claimId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (claimId == null)
                    return Unauthorized("Token inválido.");

                int idLogado = int.Parse(claimId);
                
                var envio = new  PacienteUpdateEnvioDTO{
                    Id = idLogado,
                    Nome = request.Nome,
                    Email = request.Email,
                    Telefone = request.Alergias,
                    DataNascimento = request.DataNascimento,
                    Endereco = request.Endereco,
                    HistoricoMedico = request.HistoricoMedico,
                    Alergias = request.Alergias
                };

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "atualizarpaciente",
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
