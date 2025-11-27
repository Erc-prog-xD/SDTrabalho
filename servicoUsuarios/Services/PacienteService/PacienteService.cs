using Microsoft.EntityFrameworkCore;
using servicoUsuarios.DTO;
using ServicoUsuarios.Data;
using ServicoUsuarios.DTO.Pacientes;
using ServicoUsuarios.Models;
using System.Text.Json;

public class PacienteService : IPacienteService
{
    private readonly AppDbContext _db;

    public PacienteService(AppDbContext db)
    {
        _db = db;
    }

    // ============================================================
    // REGISTRAR
    // ============================================================
    public async Task<Response<PacienteResponseDTO>> VisualizarPerfil(JsonElement dados)
    {
        Response<PacienteResponseDTO> response = new Response<PacienteResponseDTO> { Mensage = "" };
        try
        {

            var req = dados.GetInt32();


            var paciente = await _db.Users
                        .OfType<Paciente>()
                        .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);
                
            if (paciente == null)
            {
                response.Status = false;
                response.Mensage = "Paciente não encontrado.";
                return response;
            }

            var dto = new PacienteResponseDTO
            {
                Id = paciente.Id,
                Cpf = paciente.Cpf,
                Nome = paciente.Nome,
                Email = paciente.Email,
                Telefone = paciente.Telefone,
                Role = paciente.Role,
                DataNascimento = paciente.DataNascimento,
                Endereco = paciente.Endereco,
                HistoricoMedico = paciente.HistoricoMedico,
                Alergias = paciente.Alergias
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
    public async Task<Response<object>> AtualizarPerfil(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };
        try
        {
            var req = JsonSerializer.Deserialize<PacienteUpdateRequestDTO>(dados);

            if (req == null)
            {   
                response.Status = false;
                response.Mensage = "Erro no Deserialize";
                response.Dados = null;
                return response;
            }
             var paciente = await _db.Users
            .OfType<Paciente>()
            .FirstOrDefaultAsync(x => x.Id == req.Id && x.DeletionDate == null);

            if (paciente == null)
            {
                response.Status = false;
                response.Mensage = "Paciente não encontrado.";
                return response;
            }

            // Atualizar campos da classe base User
            if (!string.IsNullOrWhiteSpace(req.Nome))
                paciente.Nome = req.Nome;

            if (!string.IsNullOrWhiteSpace(req.Email))
                paciente.Email = req.Email;

            if (!string.IsNullOrWhiteSpace(req.Telefone))
                paciente.Telefone = req.Telefone;

            // Atualizar campos da classe Paciente
            if (req.DataNascimento != null)
                paciente.DataNascimento = req.DataNascimento;

            if (!string.IsNullOrWhiteSpace(req.Endereco))
                paciente.Endereco = req.Endereco;

            if (!string.IsNullOrWhiteSpace(req.HistoricoMedico))
                paciente.HistoricoMedico = req.HistoricoMedico;

            if (!string.IsNullOrWhiteSpace(req.Alergias))
                paciente.Alergias = req.Alergias;

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


    public async Task<Response<object>> DeletarPerfil(JsonElement dados)
    {
        Response<object> response = new Response<object> { Mensage = "" };
        try
        {

            var req = dados.GetInt32();


            var paciente = await _db.Users
                        .OfType<Paciente>()
                        .FirstOrDefaultAsync(x => x.Id == req && x.DeletionDate == null);
                
            if (paciente == null)
            {
                response.Status = false;
                response.Mensage = "Paciente não encontrado.";
                return response;
            }

            paciente.DeletionDate = DateTime.Now;
            
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