using Microsoft.EntityFrameworkCore;
using ServicoUsuarios.Models;

namespace ServicoUsuarios.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Notification> Notifications {get; set;}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração TPH
            modelBuilder.Entity<User>()
                .HasDiscriminator<UsertypeEnum>("Role")
                .HasValue<Paciente>(UsertypeEnum.Paciente)
                .HasValue<Medico>(UsertypeEnum.Medico)
                .HasValue<Admin>(UsertypeEnum.Admin)
                .HasValue<Recepcionista>(UsertypeEnum.Recepcionista);
        }
    }   
}
