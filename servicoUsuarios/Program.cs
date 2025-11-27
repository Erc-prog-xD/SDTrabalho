using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServicoUsuarios.Data;
using ServicoUsuarios.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        // Lê appsettings.json e sobrescreve com variáveis de ambiente se existirem
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        IConfiguration configuration = context.Configuration;

        // Pega a connection string da variável de ambiente ou do appsettings.json
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") 
                               ?? configuration.GetConnectionString("DefaultConnection");

        // Registrar DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Configurar JWT
        services.Configure<JwtConfig>(configuration.GetSection("Jwt"));

        // Registrar serviços
        
        services.AddScoped<IAuthUsuarioService, AuthUsuarioService>();
        services.AddScoped<IPacienteService, PacienteService>();
        services.AddSingleton<SocketServe>();
    })
    .Build();

// Cria o banco se não existir ou aplica migrations automaticamente
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Inicia o serviço de sockets
var server = host.Services.GetRequiredService<SocketServe>();
await server.Start();
