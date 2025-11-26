namespace EndpointsInterface.DTO
{
    public class UsuarioRegisterRequestDTO
    {
        public required string Cpf { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required UsertypeEnum Role { get; set; }
        public required string Senha { get; set; }

        // CAMPOS ESPECÍFICOS POR TIPO
        public DateTime? DataNascimento { get; set; } // Paciente
        public string? Endereco { get; set; }         // Paciente
        public string? HistoricoMedico { get; set; }  // Paciente
        public string? Alergias { get; set; }         // Paciente

        public string? CRM { get; set; }              // Médico
        public string? Especialidade { get; set; }    // Médico

        public string? Turno { get; set; }            // Recepcionista

    }
}
