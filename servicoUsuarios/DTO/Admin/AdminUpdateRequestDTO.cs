namespace ServicoUsuarios.DTO.Admin
{
    public class AdminUpdateRequestDTO
    {
        public required int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
}
