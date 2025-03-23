using DiarioPersonalApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DiarioPersonalApi.Data.Repositories
{
    public class EntradaRepository : Repository<Entrada>, IEntradaRepository
    {
        public EntradaRepository(DiarioDbContext context) : base(context) { }

        public async Task<IEnumerable<Entrada>> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .Where(e => e.UsuarioId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Entrada>> SearchByHashtagAsync(int userId, string hashtag)
        {
            return await _dbSet
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .Where(e => e.UsuarioId == userId && e.EntradasEtiquetas.Any(ee => ee.Etiqueta.Nombre == hashtag))
                .ToListAsync();
        }

        public async Task<IEnumerable<Entrada>> SearchByHashtagAdminAsync(string hashtag)
        {
            return await _dbSet
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .Where(e => e.EntradasEtiquetas.Any(ee => ee.Etiqueta.Nombre == hashtag))
                .ToListAsync();
        }
    }
}
