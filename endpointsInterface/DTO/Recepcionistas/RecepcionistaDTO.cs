
namespace EndpointsInterface.DTO.Recepcionistas
{
    public class RecepcionistaDTO 
    {
        public int Id { get; set; }
        public required string Cpf {get; set;}
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required UsertypeEnum Role { get; set; }
        public string? Turno { get; set; }
    }
}
