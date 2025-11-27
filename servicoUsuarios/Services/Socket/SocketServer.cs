using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ServicoUsuarios.DTO.Pacientes;
using ServicoUsuarios.Models;

public class SocketServe
{
    private readonly IAuthUsuarioService _authService;
    private readonly IPacienteService _pacienteService;
    private readonly TcpListener _listener;

    public SocketServe(IAuthUsuarioService authService, IPacienteService pacienteService)
    {
        _authService = authService;
        _pacienteService = pacienteService;
        _listener = new TcpListener(IPAddress.Any, 5005);
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine("Servidor de usuários rodando na porta 5005...");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = HandleClient(client);
        }
    }

    private async Task HandleClient(TcpClient client)
    {
        using var stream = client.GetStream();

        byte[] buffer = new byte[8192];
        int bytesRead = await stream.ReadAsync(buffer);

        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        using JsonDocument doc = JsonDocument.Parse(json);
        var raiz = doc.RootElement;

        string acao = raiz.GetProperty("acao").GetString();
        JsonElement dados = raiz.GetProperty("dados");

        Response<object> resp;

        switch (acao.ToLower())
        {
            case "registrar":
                resp = await _authService.Registrar(dados);
                break;

            case "login":
                resp = await _authService.Login(dados);
                break;

            case "visualizarperfil":
                resp = ConvertResponse(await _pacienteService.VisualizarPerfil(dados));
                break;

            case "atualizarpaciente":
                resp = await _pacienteService.AtualizarPerfil(dados);
                break;

            default:
                resp = new Response<object>
                {
                    Status = false,
                    Mensage = "Ação inválida!",
                    Dados = null
                };
                break;
        }
        string respostaJson = JsonSerializer.Serialize(resp);

        byte[] respostaBytes = Encoding.UTF8.GetBytes(respostaJson);
        await stream.WriteAsync(respostaBytes);
    }

    private Response<object> ConvertResponse<T>(Response<T> r)
    {
        return new Response<object>
        {
            Status = r.Status,
            Mensage = r.Mensage,
            Dados = r.Dados
        };
    }
}

