using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServicoUsuarios.Data;
using ServicoUsuarios.Models;

namespace ServicoUsuarios
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ============================
            // CONFIGURA HOST E SERVIÇOS
            // ============================
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    string? connectionString =
                    Environment.GetEnvironmentVariable("CONNECTION_STRING") ??
                    context.Configuration.GetConnectionString("DefaultConnection");

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        Console.WriteLine("❌ CONNECTION_STRING não configurada no ambiente!");
                        Environment.Exit(1);
                    }

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));
                });

            var host = builder.Build();

            // ============================
            // APLICA MIGRATIONS AUTOMÁTICAS
            // ============================
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {   
                    Console.WriteLine("🧱 Aplicando migrations...");
                    db.Database.EnsureCreated(); 
                    Console.WriteLine("✅ Migrations aplicadas com sucesso!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro ao aplicar migrations: {ex.Message}");
                    Environment.Exit(1);
                }
            }

            // ============================
            // INICIA SERVIÇO TCP
            // ============================
            using var serviceScope = host.Services.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();

            Console.WriteLine("🧠 Serviço de Usuários iniciado. Aguardando conexões...");

            TcpListener server = new TcpListener(IPAddress.Any, 5005);
            server.Start();

            while (true)
            {
                using TcpClient client = server.AcceptTcpClient();
                using NetworkStream stream = client.GetStream();

                try
                {
                    byte[] buffer = new byte[8192];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"📩 Requisição recebida: {requestJson}");

                    var envelope = JsonSerializer.Deserialize<EnvelopeRequest>(
                        requestJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );                   
                    Console.WriteLine($"📩 Envelope Dados: {envelope.Dados}");
                    Console.WriteLine($"📩 Envelope Acao: {envelope.Acao}");

                    UsuarioResponse resposta;

                    switch (envelope?.Acao?.ToLower())
                    {
                        case "registrar":
                            resposta = RegistrarUsuario(envelope.Dados, context);
                            break;

                        default:
                            resposta = new UsuarioResponse
                            {
                                Sucesso = false,
                                Mensagem = $"Ação '{envelope?.Acao}' não reconhecida."
                            };
                            break;
                    }

                    string responseJson = JsonSerializer.Serialize(resposta);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
                    stream.Write(responseBytes, 0, responseBytes.Length);

                    Console.WriteLine($"📤 Resposta enviada: {responseJson}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erro: {ex.Message}");
                }
            }
        }

        // ============================================================
        // FUNÇÃO DE REGISTRO DE USUÁRIO
        // ============================================================
        private static UsuarioResponse RegistrarUsuario(JsonElement dados, AppDbContext context)
        {
            try
            {
                var usuarioReq = JsonSerializer.Deserialize<UsuarioRegisterRequest>(dados);

                if (usuarioReq == null)
                    return new UsuarioResponse { Sucesso = false, Mensagem = "Dados inválidos no corpo da requisição." };

                // Verifica se já existe CPF cadastrado
                if (context.Users.Any(u => u.Cpf == usuarioReq.Cpf))
                {
                    return new UsuarioResponse
                    {
                        Sucesso = false,
                        Mensagem = "Usuário já cadastrado com esse CPF."
                    };
                }

                // Gera hash e salt
                CriarSenhaHash(usuarioReq.Senha, out byte[] hash, out byte[] salt);

                var novoUsuario = new User
                {
                    Cpf = usuarioReq.Cpf,
                    Nome = usuarioReq.Nome,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = usuarioReq.Role,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(novoUsuario);
                context.SaveChanges();

                Console.WriteLine($"✅ Usuário registrado com sucesso: {novoUsuario.Nome} ({novoUsuario.Cpf})");

                return new UsuarioResponse
                {
                    Sucesso = true,
                    Mensagem = "Usuário registrado com sucesso!",
                    Token = Guid.NewGuid().ToString() // token simulado
                };
            }
            catch (Exception ex)
            {
                return new UsuarioResponse
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao registrar usuário: {ex.Message}"
                };
            }
        }

        // ============================================================
        // UTILITÁRIOS DE SENHA
        // ============================================================
        private static void CriarSenhaHash(string senha, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));
        }
    }

    // ============================================================
    // CLASSES DE SUPORTE
    // ============================================================
    public class EnvelopeRequest
    {
        [JsonPropertyName("acao")]
        public string? Acao { get; set; }

        [JsonPropertyName("dados")]
        public JsonElement Dados { get; set; }
    }

    public class UsuarioRegisterRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public UsertypeEnum Role { get; set; }
    }

    public class UsuarioResponse
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
}
