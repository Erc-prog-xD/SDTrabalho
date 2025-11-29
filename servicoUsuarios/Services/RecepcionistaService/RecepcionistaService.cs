using Microsoft.EntityFrameworkCore;
using ServicoUsuarios.Data;
using ServicoUsuarios.DTO.Recepcionista;
using ServicoUsuarios.Models;
using System.Text.Json;

public class RecepcionistaService : IRecepcionistaService
{
    private readonly AppDbContext _db;

    public RecepcionistaService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // REGISTRAR
    // ============================================================
    public async Task<Response<RecepcionistaResponseDTO>> VisualizarPerfilRecepcionista(JsonElement dados)
    {
        Response<RecepcionistaResponseDTO> response = new Response<RecepcionistaResponseDTO> { Mensage = "" };
        try
        {

            var req = dados.GetInt32();


            var recepcionista = await _db.Users
                        .OfType<Recepcionista>()
                        .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);
                
            if (recepcionista == null)
            {
                response.Status = false;
                response.Mensage = "Recepcionista não encontrado.";
                return response;
            }

            var dto = new RecepcionistaResponseDTO
            {
                Id = recepcionista.Id,
                Cpf = recepcionista.Cpf,
                Nome = recepcionista.Nome,
                Email = recepcionista.Email,
                Telefone = recepcionista.Telefone,
                Role = recepcionista.Role,
                Turno = recepcionista.Turno,
            };

            response.Status = true;
            response.Mensage = "Perfil carregado com sucesso.";
            response.Dados = dto;
                
        }
        catch (Exception ex)
        {
            response.Dados = null;
            response.Status = false;
            response.Mensage = ex.Message;
        }

        return response;
    }
    public async Task<Response<object>> AtualizarPerfilRecepcionista(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };
        try
        {
            var req = JsonSerializer.Deserialize<RecepcionistaUpdateRequestDTO>(dados);

            if (req == null)
            {   
                response.Status = false;
                response.Mensage = "Erro no Deserialize";
                response.Dados = null;
                return response;
            }
             var recepcionista = await _db.Users
            .OfType<Recepcionista>()
            .FirstOrDefaultAsync(x => x.Id == req.Id && x.DeletionDate == null);

            if (recepcionista == null)
            {
                response.Status = false;
                response.Mensage = "Recepcionista não encontrado.";
                return response;
            }

            // Atualizar campos da classe base User
            if (!string.IsNullOrWhiteSpace(req.Nome))
                recepcionista.Nome = req.Nome;

            if (!string.IsNullOrWhiteSpace(req.Email))
                recepcionista.Email = req.Email;

            if (!string.IsNullOrWhiteSpace(req.Telefone))
                recepcionista.Telefone = req.Telefone;

            if (!string.IsNullOrWhiteSpace(req.Turno))
                recepcionista.Turno = req.Turno;


            await _db.SaveChangesAsync();

            response.Status = true;
            response.Dados = null;
            response.Mensage = "Perfil atualizado com sucesso";
        }
        catch (Exception ex)
        {
            response.Dados = null;
            response.Status = false;
            response.Mensage = ex.Message;
        }

        return response;
    }


    public async Task<Response<object>> DeletarPerfilRecepcionista(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };
        try
        {

            var req = dados.GetInt32();


            var recepcionista = await _db.Users
                        .OfType<Recepcionista>()
                        .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);
                
            if (recepcionista == null)
            {
                response.Status = false;
                response.Mensage = "Recepcionista não encontrado.";
                return response;
            }

            recepcionista.DeletionDate = DateTime.Now;
            
            await _db.SaveChangesAsync();

            response.Status = true;
            response.Mensage = "Perfil deletado com sucesso";
            response.Dados = null;
                
        }
        catch (Exception ex)
        {
            response.Dados = null;
            response.Status = false;
            response.Mensage = ex.Message;
        }

        return response;
    }

}