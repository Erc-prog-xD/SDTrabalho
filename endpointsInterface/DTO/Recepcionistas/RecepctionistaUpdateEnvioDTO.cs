namespace EndpointsInterface.DTO.Recepcionistas
{
    public class RecepcionistaUpdateEnvioDTO
    {
        public required int Id {get; set;}
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Turno { get; set; }
    }
}
