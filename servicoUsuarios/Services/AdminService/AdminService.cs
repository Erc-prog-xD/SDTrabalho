using Microsoft.EntityFrameworkCore;
using ServicoUsuarios.Data;
using ServicoUsuarios.DTO.Admin;
using ServicoUsuarios.Models;
using System.Text.Json;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // VISUALIZAR PERFIL ADMIN
    // ============================================================
    public async Task<Response<AdminResponseDTO>> VisualizarPerfilAdmin(JsonElement dados)
    {
        Response<AdminResponseDTO> response = new Response<AdminResponseDTO> { Mensage = "" };

        try
        {
            var req = dados.GetInt32();

            var admin = await _db.Users
                .OfType<Admin>()
                .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);

            if (admin == null)
            {
                response.Status = false;
                response.Mensage = "Admin não encontrado.";
                return response;
            }

            var dto = new AdminResponseDTO
            {
                Id = admin.Id,
                Cpf = admin.Cpf,
                Nome = admin.Nome,
                Email = admin.Email,
                Telefone = admin.Telefone,
                Role = admin.Role
            };

            response.Status = true;
            response.Mensage = "Perfil carregado com sucesso.";
            response.Dados = dto;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Mensage = ex.Message;
            response.Dados = null;
        }

        return response;
    }

    // ============================================================
    // ATUALIZAR PERFIL ADMIN
    // ============================================================
    public async Task<Response<object>> AtualizarPerfilAdmin(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };

        try
        {
            var req = JsonSerializer.Deserialize<AdminUpdateRequestDTO>(dados);

            if (req == null)
            {
                response.Status = false;
                response.Mensage = "Erro no Deserialize";
                response.Dados = null;
                return response;
            }

            var admin = await _db.Users
                .OfType<Admin>()
                .FirstOrDefaultAsync(x => x.Id == req.Id && x.DeletionDate == null);

            if (admin == null)
            {
                response.Status = false;
                response.Mensage = "Admin não encontrado.";
                return response;
            }

            if (!string.IsNullOrWhiteSpace(req.Nome))
                admin.Nome = req.Nome;

            if (!string.IsNullOrWhiteSpace(req.Email))
                admin.Email = req.Email;

            if (!string.IsNullOrWhiteSpace(req.Telefone))
                admin.Telefone = req.Telefone;

            await _db.SaveChangesAsync();

            response.Status = true;
            response.Dados = null;
            response.Mensage = "Perfil atualizado com sucesso";
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Mensage = ex.Message;
            response.Dados = null;
        }

        return response;
    }

    // ============================================================
    // DELETAR PERFIL ADMIN
    // ============================================================
    public async Task<Response<object>> DeletarPerfilAdmin(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };

        try
        {
            var req = dados.GetInt32();

            var admin = await _db.Users
                .OfType<Admin>()
                .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);

            if (admin == null)
            {
                response.Status = false;
                response.Mensage = "Admin não encontrado.";
                return response;
            }

            admin.DeletionDate = DateTime.Now;

            await _db.SaveChangesAsync();

            response.Status = true;
            response.Mensage = "Perfil deletado com sucesso";
            response.Dados = null;
        }
        catch (Exception ex)
        {
            response.Status = false;
            response.Mensage = ex.Message;
            response.Dados = null;
        }

        return response;
    }
}
