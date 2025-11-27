
namespace EndpointsInterface.DTO.Pacientes
{public class PacienteDTO 
    {
        public int Id { get; set; }
        public required string Cpf {get; set;}
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required UsertypeEnum Role { get; set; }
        public DateTime? DataNascimento { get; set; }
        public string? Endereco { get; set; }
        public string? HistoricoMedico { get; set; }
        public string? Alergias { get; set; }
    }
}
