using DiarioPersonalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DiarioPersonalApi.Data
{
    public class DiarioDbContext : DbContext
    {
        public DiarioDbContext(DbContextOptions<DiarioDbContext> options) : base(options) { }

        public DbSet<Entrada> Entradas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Etiqueta> Etiquetas { get; set; }
        public DbSet<EntradaEtiqueta> EntradasEtiquetas { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entrada>()
                .HasOne(e => e.Usuario)
                .WithMany(u => u.Entradas)
                .HasForeignKey(e => e.UsuarioId);

            modelBuilder.Entity<EntradaEtiqueta>()
                           .HasKey(e => new { e.EntradaId, e.EtiquetaId });

            modelBuilder.Entity<EntradaEtiqueta>()
                .HasOne(e => e.Entrada)
                .WithMany(e => e.EntradasEtiquetas)
                .HasForeignKey(e => e.EntradaId);

            modelBuilder.Entity<EntradaEtiqueta>()
                .HasOne(e => e.Etiqueta)
                .WithMany(e => e.EntradasEtiquetas)
                .HasForeignKey(e => e.EtiquetaId);
        }
    }
}
