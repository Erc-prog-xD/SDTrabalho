using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/recepcionista")]
    [Authorize(Roles = "Recepcionista,Admin")]
    public class RecepcionistaController : ControllerBase
    {
        [HttpGet("agendamentos")]
        public IActionResult ListarAgendamentos()
        {
            return Ok(new { Mensagem = "Listagem de agendamentos disponível." });
        }

        
        [HttpPost("agendar")]
        public IActionResult CriarAgendamento([FromBody] object agendamento)
        {
            return Ok(new { Mensagem = "Agendamento criado com sucesso." });
        }
    }
}
