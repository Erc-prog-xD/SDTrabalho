using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ServicoUsuarios.DTO.Paciente;
using ServicoUsuarios.Models;

public class SocketServe
{
    private readonly IAuthUsuarioService _authService;
    private readonly IPacienteService _pacienteService;

    private readonly IRecepcionistaService _recepcionistaService;
    private readonly IMedicoService _medicoService;
    private readonly TcpListener _listener;

    public SocketServe(IAuthUsuarioService authService, IPacienteService pacienteService, IMedicoService medicoService, IRecepcionistaService recepcionistaService)
    {
        _authService = authService;
        _medicoService = medicoService;
        _pacienteService = pacienteService;
        _recepcionistaService = recepcionistaService;
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

            case "visualizarperfilpaciente":
                resp = ConvertResponse(await _pacienteService.VisualizarPerfilPaciente(dados));
                break;
            case "deletarperfilpaciente":
                resp = await _pacienteService.DeletarPerfilPaciente(dados);
                break;
            case "atualizarpaciente":
                resp = await _pacienteService.AtualizarPerfilPaciente(dados);
                break;

            case "visualizarperfilmedico":
                resp = ConvertResponse(await _medicoService.VisualizarPerfilMedico(dados));
                break;
            case "deletarperfilmedico":
                resp = await _medicoService.DeletarPerfilMedico(dados);
                break;
            case "atualizarmedico":
                resp = await _medicoService.AtualizarPerfilMedico(dados);
                break;

            case "visualizarperfilrecepcionista":
                resp = ConvertResponse(await _recepcionistaService.VisualizarPerfilRecepcionista(dados));
                break;
            case "deletarperfilrecepcionista":
                resp = await _recepcionistaService.DeletarPerfilRecepcionista(dados);
                break;
            case "atualizarrecepcionista":
                resp = await _recepcionistaService.AtualizarPerfilRecepcionista(dados);
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

