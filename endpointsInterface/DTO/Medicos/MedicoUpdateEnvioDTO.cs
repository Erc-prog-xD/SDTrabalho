namespace EndpointsInterface.DTO.Medico
{
    public class MedicoUpdateEnvioDTO
    {
        public required int Id {get; set;}
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? CRM { get; set; }
        public string? Especialidade { get; set; }
    }
}
