using Microsoft.EntityFrameworkCore;
using ServicoUsuarios.Models;

namespace ServicoUsuarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // tabela de usuários
    }
}
