using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServicoUsuarios.Data;
using ServicoUsuarios.DTO;
using ServicoUsuarios.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

public class AuthUsuarioService : IAuthUsuarioService
{
    private readonly AppDbContext _db;
    private readonly JwtConfig _jwt;

    public AuthUsuarioService(AppDbContext db, IOptions<JwtConfig> jwtOptions)
    {
        _db = db;
        _jwt = jwtOptions.Value;
    }

    // ============================================================
    // REGISTRAR
    // ============================================================
    public async Task<Response<string>> Registrar(JsonElement dados)
    {
        Response<string> response = new Response<string> { Mensage = "" };
        try
        {
            var req = JsonSerializer.Deserialize<UsuarioRegisterRequestDTO>(dados);

            if (req == null)
            {   
                response.Status = false;
                response.Mensage = "Erro no Deserialize";
                return response;
            }

            if (await _db.Users.AnyAsync(u => u.Cpf == req.Cpf)){
                response.Status = false;
                response.Mensage = "Cpf ou senha ja cadastrado!";
                return response;
            }
            CriarHash(req.Senha, out byte[] hash, out byte[] salt);

            User novo = req.Role switch
            {
                UsertypeEnum.Paciente => new Paciente
                {
                    Cpf = req.Cpf,
                    Nome = req.Nome,
                    Email = req.Email,
                    Telefone = req.Telefone,
                    DataNascimento = req.DataNascimento,
                    Alergias = req.Alergias,
                    Endereco = req.Endereco,
                    HistoricoMedico = req.HistoricoMedico,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = req.Role
                },
                UsertypeEnum.Medico => new Medico
                {
                    Cpf = req.Cpf,
                    Nome = req.Nome,
                    Email = req.Email,
                    Telefone = req.Telefone,
                    CRM = req.CRM,
                    Especialidade = req.Especialidade,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = req.Role
                },
                UsertypeEnum.Recepcionista => new Recepcionista
                {
                    Cpf = req.Cpf,
                    Nome = req.Nome,
                    Email = req.Email,
                    Telefone = req.Telefone,
                    Turno = req.Turno,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = req.Role
                },
                UsertypeEnum.Admin => new Admin
                {
                    Cpf = req.Cpf,
                    Nome = req.Nome,
                    Email = req.Email,
                    Telefone = req.Telefone,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                    Role = req.Role
                },
                _ => null!
            };

            if (novo == null)
            {
                response.Status = false;
                response.Mensage = "Role inválida!";
                return response;
            }

            _db.Users.Add(novo);
            await _db.SaveChangesAsync();
            
            response.Status = false;
            response.Mensage = "Usuário registrado com sucesso!";
        }
        catch (Exception ex)
        {
            response.Dados = null;
            response.Status = false;
            response.Mensage = ex.Message;
        }

        return response;
    }

    // ============================================================
    // LOGIN
    // ============================================================
    public async Task<Response<string>> Login(JsonElement dados)
    {
        Response<string> response = new Response<string> { Mensage = "" };

        try
        {
            var req = JsonSerializer.Deserialize<LoginRequest>(dados);

            if (req == null)
            {
                response.Status = false;
                response.Mensage = "Dados inválidos.";
                return response;
            }
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Cpf == req.Cpf);

            if (user == null)
            {
                response.Status = false;
                response.Mensage = "CPF ou senha inválidos.";
                return response;
            }
            if (!VerificarSenha(req.Senha, user.PasswordHash, user.PasswordSalt))
            {
                response.Status = false;
                response.Mensage = "CPF ou senha inválidos.";
                return response;
            }
            string token = GerarJwt(user);

            response.Status = true;
            response.Mensage = "Login realizado com sucesso!";
            response.Dados = token;
        }
        catch (Exception ex)
        {   
            response.Status = false;
            response.Mensage = $"Erro no login: {ex.Message}";
        }
        return response;
    }

    // ============================================================
    // UTILITÁRIOS: Senha e JWT
    // ============================================================
    private void CriarHash(string senha, out byte[] hash, out byte[] salt)
    {
        using var h = new HMACSHA512();
        salt = h.Key;
        hash = h.ComputeHash(Encoding.UTF8.GetBytes(senha));
    }

    private bool VerificarSenha(string senha, byte[] hash, byte[] salt)
    {
        using var h = new HMACSHA512(salt);
        var calc = h.ComputeHash(Encoding.UTF8.GetBytes(senha));
        return calc.SequenceEqual(hash);
    }

    private string GerarJwt(User user)
    {
        var key = Encoding.ASCII.GetBytes(_jwt.Secret);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

        var desc = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Subject = new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim("id", user.Id.ToString()),
                new System.Security.Claims.Claim("cpf", user.Cpf),
                new System.Security.Claims.Claim("role", user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(_jwt.ExpiresInHours),
            SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(desc);
        return handler.WriteToken(token);
    }
}

 public class LoginRequest
{
    public string Cpf { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}