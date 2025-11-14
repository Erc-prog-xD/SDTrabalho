public class PacienteConvenio
{
    public int Id { get; set; }
    // FK Paciente
    public required Paciente Paciente { get; set; }
    // FK Convênio
    public required Convenio Convenio { get; set; }

    public string? NumeroCarteirinhaConvenio { get; set; }
    public DateTime? Validade { get; set; }
    public bool Ativo { get; set; } = true;
}
