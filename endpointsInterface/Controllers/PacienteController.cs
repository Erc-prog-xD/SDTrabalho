using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/paciente")]
    [Authorize(Roles = "Paciente,Admin")] // Apenas Paciente ou Admin
    public class PacienteController : ControllerBase
    {
      
        [HttpGet("visualizarPerfil")]
        public IActionResult VisualizarPerfil()
        {
            // Aqui você poderia usar o Claim "cpf" ou "id" do token para buscar dados do paciente
            var cpf = User.Claims.FirstOrDefault(c => c.Type == "cpf")?.Value;
            return Ok(new { Mensagem = $"Perfil do paciente {cpf}" });
        }

    
        [HttpPut("atualizarPerfil")]
        public IActionResult AtualizarPerfil([FromBody] object dadosPaciente)
        {
            return Ok(new { Mensagem = "Atualização de perfil do paciente realizada com sucesso." });
        }
    }
}
