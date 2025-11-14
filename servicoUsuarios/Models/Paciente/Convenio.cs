public class Convenio
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    // Ex.: "Básico", "Especial", "Premium"
    public ConvenioPlanoEnum? Plano { get; set; }
    public bool Ativo { get; set; } = true;
    // Relacionamento N:N
    public ICollection<PacienteConvenio>? Pacientes { get; set; }
}
