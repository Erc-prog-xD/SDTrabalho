using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/medico")]
    [Authorize(Roles = "Medico,Admin")]
    public class MedicoController : ControllerBase
    {
        [HttpGet("agenda")]
        public IActionResult ObterAgenda()
        {
            var cpf = User.Claims.FirstOrDefault(c => c.Type == "cpf")?.Value;
            return Ok(new { Mensagem = $"Agenda do médico {cpf}" });
        }

        [HttpPost("atender")]
        public IActionResult RegistrarAtendimento([FromBody] object atendimento)
        {
            return Ok(new { Mensagem = "Atendimento registrado com sucesso." });
        }
    }
}
