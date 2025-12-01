namespace EndpointsInterface.DTO.Admins
{
    public class AdminUpdateEnvioDTO
    {
        public required int Id { get; set; }
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
    }
}
