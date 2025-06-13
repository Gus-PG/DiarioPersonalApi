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

        public async Task<IEnumerable<Entrada>> SearchByFiltroAsync(FiltroBusquedaDTO filtro, int? userId = null)
        {
            var query = _dbSet
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)               
                .AsQueryable();

            // Filtrar por usuario si no es admin
            if (userId.HasValue)            
                query = query.Where(e => e.UsuarioId == userId.Value);


            // Filtrado por Fecha
            if (filtro.FechaDesde.HasValue)
                query = query.Where(e => e.Fecha >= filtro.FechaDesde.Value);

            if (filtro.FechaHasta.HasValue)
                query = query.Where(e => e.Fecha <= filtro.FechaHasta.Value);


            // Filtrado por texto
            if (!string.IsNullOrWhiteSpace(filtro.Texto))
                query = query.Where(e => e.Contenido.Contains(filtro.Texto));


            // Filtrado por etiquetas (AND/OR)
            if (filtro.Etiquetas != null && filtro.Etiquetas.Any())
            {
                if (filtro.EsBusquedaAnd)
                {
                    // Todas las etiquetas deben estar presentes (AND)
                    foreach (var etiqueta in filtro.Etiquetas)
                    {
                        var temp = etiqueta;
                        query = query.Where(e => e.EntradasEtiquetas.Any(ee => ee.Etiqueta.Nombre == temp));
                    }
                }
                else
                {
                    // Al menos una etiqueta (OR)
                    query = query.Where(e =>
                        e.EntradasEtiquetas.Any(ee => filtro.Etiquetas.Contains(ee.Etiqueta.Nombre)));
                }
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
