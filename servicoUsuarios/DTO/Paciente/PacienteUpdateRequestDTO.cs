namespace ServicoUsuarios.DTO.Paciente
{
    public class PacienteUpdateRequestDTO
    {
        public required int Id {get; set;}
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public DateTime? DataNascimento { get; set; } // Paciente
        public string? Endereco { get; set; }         // Paciente
        public string? HistoricoMedico { get; set; }  // Paciente
        public string? Alergias { get; set; }         // Paciente
    }
}
