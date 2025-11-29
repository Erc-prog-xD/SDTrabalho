using Microsoft.EntityFrameworkCore;
using ServicoUsuarios.Data;
using ServicoUsuarios.DTO.Medico;
using ServicoUsuarios.Models;
using System.Text.Json;

public class MedicoService : IMedicoService
{
    private readonly AppDbContext _db;

    public MedicoService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // REGISTRAR
    // ============================================================
    public async Task<Response<MedicoResponseDTO>> VisualizarPerfilMedico(JsonElement dados)
    {
        Response<MedicoResponseDTO> response = new Response<MedicoResponseDTO> { Mensage = "" };
        try
        {

            var req = dados.GetInt32();


            var medico = await _db.Users
                        .OfType<Medico>()
                        .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);
                
            if (medico == null)
            {
                response.Status = false;
                response.Mensage = "Medico não encontrado.";
                return response;
            }

            var dto = new MedicoResponseDTO
            {
                Id = medico.Id,
                Cpf = medico.Cpf,
                Nome = medico.Nome,
                Email = medico.Email,
                Telefone = medico.Telefone,
                Role = medico.Role,
                CRM = medico.CRM,
                Especialidade = medico.Especialidade
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
    public async Task<Response<object>> AtualizarPerfilMedico(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };
        try
        {
            var req = JsonSerializer.Deserialize<MedicoUpdateRequestDTO>(dados);

            if (req == null)
            {   
                response.Status = false;
                response.Mensage = "Erro no Deserialize";
                response.Dados = null;
                return response;
            }
             var medico = await _db.Users
            .OfType<Medico>()
            .FirstOrDefaultAsync(x => x.Id == req.Id && x.DeletionDate == null);

            if (medico == null)
            {
                response.Status = false;
                response.Mensage = "Medico não encontrado.";
                return response;
            }

            // Atualizar campos da classe base User
            if (!string.IsNullOrWhiteSpace(req.Nome))
                medico.Nome = req.Nome;

            if (!string.IsNullOrWhiteSpace(req.Email))
                medico.Email = req.Email;

            if (!string.IsNullOrWhiteSpace(req.Telefone))
                medico.Telefone = req.Telefone;

            if (!string.IsNullOrWhiteSpace(req.CRM))
                medico.CRM = req.CRM;

            if (!string.IsNullOrWhiteSpace(req.Especialidade))
                medico.Especialidade = req.Especialidade;


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


    public async Task<Response<object>> DeletarPerfilMedico(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };
        try
        {

            var req = dados.GetInt32();


            var medico = await _db.Users
                        .OfType<Medico>()
                        .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);
                
            if (medico == null)
            {
                response.Status = false;
                response.Mensage = "Medico não encontrado.";
                return response;
            }

            medico.DeletionDate = DateTime.Now;
            
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