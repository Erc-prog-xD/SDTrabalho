using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        public static async Task Main(string[] args)
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
                    
                    // Configura JWT
                    string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ??
                       context.Configuration["Jwt:Secret"] ??
                       throw new Exception("JWT_SECRET não configurada!");
                    
                    services.AddSingleton(new JwtConfig
                    {
                        Secret = jwtSecret,
                        Issuer = context.Configuration["Jwt:Issuer"] ?? "ServicoUsuarios",
                        Audience = context.Configuration["Jwt:Audience"] ?? "Clientes",
                        ExpiresInHours = int.Parse(context.Configuration["Jwt:ExpiresInHours"] ?? "2")
                    });
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
            var jwtConfig = serviceScope.ServiceProvider.GetRequiredService<JwtConfig>();

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

                    Response resposta;

                    switch (envelope?.Acao?.ToLower())
                    {
                        case "registrar":
                            resposta = await RegistrarUsuario(envelope.Dados, context);
                            break;
                        case "login":
                            resposta = await LoginUsuario(envelope.Dados, context, jwtConfig);
                            break;
                        case "atualizarpaciente":
                            resposta = await AtualizarPaciente(envelope.Dados, context);
                            break;
                        case "adicionarpaciente":
                            resposta = await AdicionarPaciente(envelope.Dados, context);
                            break;
                        default:
                            resposta = new Response
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
        private static async Task<Response> RegistrarUsuario(JsonElement dados, AppDbContext context)
        {
            try
            {
                var usuarioReq = JsonSerializer.Deserialize<UsuarioRegisterRequest>(dados);

                if (usuarioReq == null)
                    return new Response { Sucesso = false, Mensagem = "Dados inválidos no corpo da requisição." };

                // Verifica se já existe CPF cadastrado
                if (await context.Users.AnyAsync(u => u.Cpf == usuarioReq.Cpf))
                {
                    return new Response
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

                await context.Users.AddAsync(novoUsuario);
                await context.SaveChangesAsync();

                return new Response
                {
                    Sucesso = true,
                    Mensagem = "Usuário registrado com sucesso!",
                    Token = null // não gera JWT no registro
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao registrar usuário: {ex.Message}"
                };
            }
        }


        private static async Task<Response> LoginUsuario(JsonElement dados, AppDbContext context, JwtConfig config)
        {
            try
            {
                var loginReq = JsonSerializer.Deserialize<LoginRequest>(dados);

                if (loginReq == null)
                    return new Response { Sucesso = false, Mensagem = "Dados inválidos no corpo da requisição." };

                var user = await context.Users.FirstOrDefaultAsync(u => u.Cpf == loginReq.Cpf);

                if (user == null || !VerificarSenha(loginReq.Senha, user.PasswordHash, user.PasswordSalt))
                    return new Response { Sucesso = false, Mensagem = "CPF ou senha inválidos." };

                var token = GerarJwt(user, config);

                return new Response
                {
                    Sucesso = true,
                    Token = token,
                    Mensagem = "Login realizado com sucesso!"
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao realizar login: {ex.Message}"
                };
            }
        }

        private static async Task<Response> AtualizarPaciente(JsonElement dados, AppDbContext context)
        {
            try
            {
                // Desserializa JSON enviado no socket
                var req = JsonSerializer.Deserialize<PacienteUpdateEnvio>(dados);

                if (req == null)
                {
                    return new Response
                    {
                        Sucesso = false,
                        Mensagem = "Dados inválidos na requisição."
                    };
                }

                // Verifica se o usuário existe
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == req.IdLogado && u.DeletionDate.ToString() == null);

                if (user == null)
                {
                    return new Response
                    {
                        Sucesso = false,
                        Mensagem = "Usuário informado não existe."
                    };
                }

                // Verifica se já existe paciente vinculado ao usuário
                var paciente = await context.Pacientes.FirstOrDefaultAsync(p => p.User.Id == user.Id && p.DeletionDate.ToString() == null);

                if (paciente != null)
                {
                    // === Atualiza paciente existente ===
                    paciente.Contato = req.Contato;
                    paciente.DataNascimento = req.DataNascimento;
                    paciente.HistoricoMedico = req.HistoricoMedico;

                    context.Pacientes.Update(paciente);
                    await context.SaveChangesAsync();

                }
                else
                {
                    return new Response
                    {
                        Sucesso = false,
                        Mensagem = "Paciente não existe."
                    };
                }
                    return new Response
                    {
                        Sucesso = true,
                        Mensagem = "Paciente atualizado com sucesso."
                    };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao atualizar paciente: {ex.Message}"
                };
            }
        }
        private static async Task<Response> AdicionarPaciente(JsonElement dados, AppDbContext context)
        {
            try
            {
                // Desserializa JSON enviado no socket
                var req = JsonSerializer.Deserialize<PacienteUpdateEnvio>(dados);

                if (req == null)
                {
                    return new Response
                    {
                        Sucesso = false,
                        Mensagem = "Dados inválidos na requisição."
                    };
                }

                // Verifica se o usuário existe
                var user = await context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == req.IdLogado && u.DeletionDate.ToString() == null);

                if (user == null)
                {
                    return new Response
                    {
                        Sucesso = false,
                        Mensagem = "Usuário informado não existe."
                    };
                }

                // Verifica se já existe paciente vinculado ao usuário
                var paciente = await context.Pacientes.FirstOrDefaultAsync(p => p.User.Id == user.Id && p.DeletionDate.ToString() == null);

                if(paciente != null)
                {
                     return new Response
                    {
                        Sucesso = false,
                        Mensagem = "Paciente já existe."
                    };
                }
             
                // === Cria novo paciente ===
                var novoPaciente = new Paciente
                    {
                        User = user,
                        Contato = req.Contato,
                        DataNascimento = req.DataNascimento,
                        HistoricoMedico = req.HistoricoMedico
                    };

                await context.Pacientes.AddAsync(novoPaciente);
                await context.SaveChangesAsync();
                
                return new Response
                    {
                        Sucesso = true,
                        Mensagem = "Paciente criado com sucesso."
                    };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    Sucesso = false,
                    Mensagem = $"Erro ao atualizar paciente: {ex.Message}"
                };
            }
        }

        private static string GerarJwt(User user, JwtConfig config)
        {
            var key = Encoding.ASCII.GetBytes(config.Secret);
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim("id", user.Id.ToString()),
                    new System.Security.Claims.Claim("cpf", user.Cpf.ToString()),
                    new System.Security.Claims.Claim("nome", user.Nome.ToString()),
                    new System.Security.Claims.Claim("role", user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(config.ExpiresInHours),
                Issuer = config.Issuer,
                Audience = config.Audience,
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
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

        private static bool VerificarSenha(string senha, byte[] hashArmazenado, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var hashCalculado = hmac.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return hashCalculado.SequenceEqual(hashArmazenado);
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
    public class JwtConfig
    {
        public string Secret { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiresInHours { get; set; }
    }
    public class UsuarioRegisterRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public UsertypeEnum Role { get; set; }
    }
    public class LoginRequest
    {
        public string Cpf { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
    public class Response
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string? Token { get; set; }
    }
    public class PacienteUpdateEnvio
{
    public required string IdLogado { get; set; }
    public string? Contato { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? HistoricoMedico { get; set; }
}
}
