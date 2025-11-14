using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EndpointsInterface.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("usuarios")]
        public IActionResult ListarUsuarios()
        {
            return Ok(new { Mensagem = "Listagem de todos os usuários." });
        }

        [HttpDelete("usuario/{cpf}")]
        public IActionResult RemoverUsuario(string cpf)
        {
            return Ok(new { Mensagem = $"Usuário {cpf} removido com sucesso." });
        }
    }
}
