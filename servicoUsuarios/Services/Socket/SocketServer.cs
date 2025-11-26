using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ServicoUsuarios.Models;

public class SocketServe
{
    private readonly IAuthUsuarioService _authService;
    private readonly TcpListener _listener;

    public SocketServe(IAuthUsuarioService authService)
    {
        _authService = authService;
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

        Response<string> resp;

        if (acao == "registrar")
            resp = await _authService.Registrar(dados);
        else if (acao == "login")
            resp = await _authService.Login(dados);
        else
        {
            resp = new Response<string>
            {
                Status = false,
                Mensage = "Ação inválida!"
            };
        }

        string respostaJson = JsonSerializer.Serialize(resp);

        byte[] respostaBytes = Encoding.UTF8.GetBytes(respostaJson);
        await stream.WriteAsync(respostaBytes);
    }
}
