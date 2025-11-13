using System.ComponentModel.DataAnnotations;

namespace ServicoUsuarios.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Cpf {get; set;}
        public required string Nome { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public required UsertypeEnum Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletionDate {get; set;} = null;
    }
}
