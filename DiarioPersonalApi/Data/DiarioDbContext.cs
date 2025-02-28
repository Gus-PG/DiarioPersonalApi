using Microsoft.EntityFrameworkCore;

namespace DiarioPersonalApi.Data
{
    public class DiarioDbContext : DbContext
    {
        public DiarioDbContext(DbContextOptions<DiarioDbContext> options) : base(options) { }

        public DbSet<Entrada> Entradas { get; set; }
    }
}
