
namespace ServicoUsuarios.DTO.Recepcionista
{
    public class RecepcionistaResponseDTO 
    {
        public int Id { get; set; }
        public required string Cpf {get; set;}
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required UsertypeEnum Role { get; set; }
        public required string Turno { get; set; }
    }
}
