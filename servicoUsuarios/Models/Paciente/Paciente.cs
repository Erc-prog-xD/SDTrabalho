using ServicoUsuarios.Models;

public class Paciente
{
    public int Id { get; set; } // PK da tabela Paciente
    public User User { get; set; } = null!; // Navegação para o usuário
    public string? Contato { get; set; }
    public DateTime DataNascimento {get; set;}
    public string? HistoricoMedico {get; set;}
    public ICollection<PacienteConvenio>? Convenios { get; set; }
    public DateTime? DeletionDate {get; set;} = null;

}