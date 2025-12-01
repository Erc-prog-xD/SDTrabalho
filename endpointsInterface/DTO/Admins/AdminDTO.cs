namespace EndpointsInterface.DTO.Admins
{
    public class AdminDTO
    {
        public int Id { get; set; }
        public required string Cpf { get; set; }
        public required string Nome { get; set; }
        public required string Email { get; set; }
        public required string Telefone { get; set; }
        public required UsertypeEnum Role { get; set; }
    }
}
