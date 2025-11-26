namespace EndpointsInterface.Models
{
    public class Response<T>
    {
        public T? Dados {  get; set; }
        public string? Mensage {  get; set; }
        public bool Status { get; set; } = true;
    }
}
