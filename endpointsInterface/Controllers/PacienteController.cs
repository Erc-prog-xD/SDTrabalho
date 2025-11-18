using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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
        public IActionResult VisualizarPerfil()
        {
            // Aqui você poderia usar o Claim "cpf" ou "id" do token para buscar dados do paciente
            var cpf = User.Claims.FirstOrDefault(c => c.Type == "cpf")?.Value;
            return Ok(new { Mensagem = $"Perfil do paciente {cpf}" });
        }



        [HttpPost("cadastrarPerfil")]
        public async Task<IActionResult> cadastrarPerfil([FromBody] PacienteUpdateRequest request)
        {
            try
            {
                var Id = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (Id == null)
                    return Unauthorized("Token inválido.");

                var newRequest = new PacienteEnvio
                {
                    IdLogado = Id,
                    Contato = request.Contato,
                    DataNascimento = request.DataNascimento,
                    HistoricoMedico = request.HistoricoMedico
                };

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "adicionarpaciente",
                    dados = newRequest
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                 // Aguarda resposta do serviço
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<PacienteResponse>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }

        [HttpPut("atualizarPerfil")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] PacienteUpdateRequest request)
        {
            try
            {
                var Id = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
                if (Id == null)
                    return Unauthorized("Token inválido.");

                var newRequest = new PacienteEnvio
                {
                    IdLogado = Id,
                    Contato = request.Contato,
                    DataNascimento = request.DataNascimento,
                    HistoricoMedico = request.HistoricoMedico
                };

                using TcpClient client = new TcpClient();
                await client.ConnectAsync(_usuarioHost, _usuarioPort);
                using NetworkStream stream = client.GetStream();

                var envelope = new
                {
                    acao = "atualizarpaciente",
                    dados = newRequest
                };

                string json = JsonSerializer.Serialize(envelope);
                byte[] data = Encoding.UTF8.GetBytes(json);
                await stream.WriteAsync(data, 0, data.Length);

                 // Aguarda resposta do serviço
                byte[] buffer = new byte[8192];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var response = JsonSerializer.Deserialize<PacienteResponse>(responseJson);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = $"Erro ao comunicar com o serviço de usuários: {ex.Message}" });
            }
        }
    }

    public class PacienteUpdateRequest
    {
        public string? Contato { get; set; }
        public required DateTime DataNascimento { get; set; }
        public string? HistoricoMedico { get; set; }
        
    }
    public class PacienteEnvio
    {
        public required string IdLogado {get;set;}
        public string? Contato { get; set; }
        public required DateTime DataNascimento { get; set; }
        public string? HistoricoMedico { get; set; }
        
    }

     public class PacienteResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}
