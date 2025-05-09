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

        public async Task<Entrada?> GetEntradaByIdWithUsuarioAsync(int id)
        {
            return await _dbSet
                .Include(e => e.Usuario)  // 👈 Aquí traes la navegación
                .Include(e => e.EntradasEtiquetas)
                    .ThenInclude(ee => ee.Etiqueta)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<string>> GetEtiquetasUsuarioAsync(int userId) 
        {
            return await _context.EntradasEtiquetas
                .Where(ee => ee.Entrada.UsuarioId == userId)
                .Select(ee => ee.Etiqueta.Nombre)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<Entrada>> SearchByTextoYOEtiquetasAsync(string texto, List<string> etiquetas, int? userId = null)
        {
            var query = _dbSet
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)               
                .AsQueryable();

            // Si se especifica un usuario, filtrar por él
            if (userId.HasValue)
            {
                query = query.Where(e => e.UsuarioId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(texto))
            {
                query = query.Where(e => e.Contenido.Contains(texto));
            }

            if (etiquetas != null && etiquetas.Any())
            {
                query = query.Where(e =>
                    e.EntradasEtiquetas.Any(ee => etiquetas.Contains(ee.Etiqueta.Nombre)));
            }

            return await query.ToListAsync();
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

        public async Task<List<EntradaPreviewDTO>> GetPreviewEntradasPaginadoAsync(int userId, int page, int pageSize)
        {
            return await _dbSet
                .Where(e => e.UsuarioId == userId)
                .OrderByDescending(e => e.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EntradaPreviewDTO
                {
                    Id = e.Id,
                    Fecha = e.Fecha,
                    Preview = e.Contenido.Substring(0, Math.Min(250, e.Contenido.Length)),
                    Etiquetas = e.EntradasEtiquetas.Select(ee => ee.Etiqueta.Nombre).ToList()
                })
                .ToListAsync();
        }
    }
}
