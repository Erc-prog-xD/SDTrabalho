using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using NotificationService.models;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly IConfiguration _config;

    private IConnection? _connection;
    private IModel? _channel;
    private NotificationRepository? _repo;

    public NotificationWorker(ILogger<NotificationWorker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        // SQL
        var sqlConn =
            Environment.GetEnvironmentVariable("SQL_CONN")
            ?? _config.GetConnectionString("Sql")
            ?? throw new Exception("SQL_CONN não definido (env ou appsettings ConnectionStrings:Sql).");

        _repo = new NotificationRepository(sqlConn);

        // Rabbit
        var host =
            Environment.GetEnvironmentVariable("RABBIT_HOST")
            ?? _config["Rabbit:Host"]
            ?? "localhost";

        var user =
            Environment.GetEnvironmentVariable("RABBIT_USER")
            ?? _config["Rabbit:User"]
            ?? "guest";

        var pass =
            Environment.GetEnvironmentVariable("RABBIT_PASS")
            ?? _config["Rabbit:Pass"]
            ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass
        };

        _connection = factory.CreateConnection("notificationservice-publisher");
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: "notifications",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation("✅ Publisher pronto: SQL ok + RabbitMQ ok ({Host}).", host);

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_repo is null) throw new InvalidOperationException("Repo não inicializado.");
        if (_channel is null) throw new InvalidOperationException("Channel não inicializado.");

        var pollSeconds = int.TryParse(_config["Outbox:PollSeconds"], out var s) ? s : 2;
        var take = int.TryParse(_config["Outbox:Take"], out var t) ? t : 50;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pending = await _repo.GetPendingAsync(take);

                if (pending.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(pollSeconds), stoppingToken);
                    continue;
                }

                foreach (var n in pending)
                {
                    // publica no Rabbit
                    var json = JsonSerializer.Serialize(n);
                    var body = Encoding.UTF8.GetBytes(json);

                    var props = _channel.CreateBasicProperties();
                    props.Persistent = true; // importante se fila for durable
                    props.ContentType = "application/json";

                    _channel.BasicPublish(
                        exchange: "",
                        routingKey: "notifications",
                        basicProperties: props,
                        body: body);

                    // marca como publicado no banco
                    await _repo.MarkPublishedAsync(n.Id);

                    _logger.LogInformation("📤 Publicado e marcado Published=1 (Id={Id}, AppointmentId={AppointmentId})",
                        n.Id, n.AppointmentId);
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown normal
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erro no publisher loop. Vai tentar de novo.");
                await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return base.StopAsync(cancellationToken);
    }
}
