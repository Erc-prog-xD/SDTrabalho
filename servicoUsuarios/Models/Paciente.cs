
namespace ServicoUsuarios.Models
{
    public class Paciente : User
    {
        public DateTime? DataNascimento { get; set; }
        public string? Endereco { get; set; }
        public string? HistoricoMedico { get; set; }
        public string? Alergias { get; set; }
    }
}
