using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiarioPersonalApi.Data;
using DiarioPersonalApi.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace DiarioPersonalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere token para todos los métodos
    public class EntradasController : ControllerBase
    {
        private readonly DiarioDbContext _db;

        public EntradasController(DiarioDbContext db)
        {
            _db = db;
        }


        #region " USUARIOS. "

        // GET: api/entradas/usuario/{userId}
        [HttpGet("usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<EntradaResponseDTO>>> GetEntradasUsuario(int userId)
        {
            var entradas = await _db.Entradas
               .Where(e => e.UsuarioId == userId)
               .Include(e => e.Usuario)
               .Include(e => e.EntradasEtiquetas)
               .ThenInclude(ee => ee.Etiqueta)
               .ToListAsync();

            var response = entradas.Select(entrada => new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            });

            return Ok(response);
        }



        // GET: api/entradas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EntradaResponseDTO>> GetEntradaUsuario(int id)
        {
            var entrada = await _db.Entradas
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entrada == null) return NotFound();

            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            };

            return Ok(response);
        }



        // GET: api/entradas/etiquetas/{tag}
        [HttpGet("etiqueta/{tag}")]
        public async Task<ActionResult<IEnumerable<Entrada>>> GetEntradasPorEtiqueta(string tag)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var entradas = await _db.Entradas
                .Where(e => e.UsuarioId == userId && e.EntradasEtiquetas.Any(ee => ee.Etiqueta.Nombre == tag))
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            var response = entradas.Select(entrada => new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            });

            return Ok(response);
        }



        // POST: api/entradas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EntradaResponseDTO>> PostEntradaUsuario([FromBody] EntradaRequestDTO entradaDTO)
        {
            // Validar que el usuario exista.
            //var usuario = await _db.Usuarios.FindAsync(entradaDTO.UserId);
            //if (usuario == null) return BadRequest("Usuario no encontrado");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var entrada = new Entrada
            {
                UsuarioId = userId,    // Del Token, no del DTO.
                Fecha = entradaDTO.Fecha,
                Contenido = entradaDTO.Contenido
            };

            // La fecha viene del front, no lo asignamos aquí.
            _db.Entradas.Add(entrada);

            // Parsear hashtags
            var hashtags = Regex.Matches(entrada.Contenido, @"#\w+")
                .Select(m => m.Value.Substring(1))
                .Distinct();
            foreach (var tag in hashtags)
            {
                var etiqueta = await _db.Etiquetas.FirstOrDefaultAsync(e => e.Nombre == tag)
                    ?? new Etiqueta { Nombre = tag };
                if (etiqueta.Id == 0) _db.Etiquetas.Add(etiqueta);
                _db.EntradasEtiquetas.Add(new EntradaEtiqueta
                {
                    Entrada = entrada,
                    Etiqueta = etiqueta
                });
            }

            await _db.SaveChangesAsync();

            var usuario = await _db.Usuarios.FindAsync(userId);
            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = usuario?.NombreUsuario ?? "",
                Etiquetas = hashtags.ToList()
            };

            return CreatedAtAction(nameof(GetEntradaUsuario), new { id = entrada.Id }, response);
        }



        // PUT: api/entradas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntrada(int id, [FromBody] EntradaRequestDTO entradaDTO)
        {
            var entrada = await _db.Entradas
                .Include(e => e.EntradasEtiquetas)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            // Comprobaciones.
            if (entrada == null) return NotFound();
            if (entrada.UsuarioId != entradaDTO.UserId) return Forbid("No autorizado");
            entrada.Fecha = entradaDTO.Fecha;
            entrada.Contenido = entradaDTO.Contenido;


            // Actualizar hashtags
            _db.EntradasEtiquetas.RemoveRange(entrada.EntradasEtiquetas ?? new List<EntradaEtiqueta>());
            var hashtags = Regex.Matches(entrada.Contenido, @"#\w+")
                .Select(m => m.Value.Substring(1))
                .Distinct();
            foreach (var tag in hashtags)
            {
                var etiqueta = await _db.Etiquetas.FirstOrDefaultAsync(e => e.Nombre == tag)
                    ?? new Etiqueta { Nombre = tag };
                if (etiqueta.Id == 0) _db.Etiquetas.Add(etiqueta);
                _db.EntradasEtiquetas.Add(new EntradaEtiqueta
                {
                    EntradaId = entrada.Id,
                    Etiqueta = etiqueta
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }



        // DELETE: api/Entradas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntrada(int id)
        {
            var entrada = await _db.Entradas.FindAsync(id);
            if (entrada == null) return NotFound();

            _db.Entradas.Remove(entrada);
            await _db.SaveChangesAsync();
            return NoContent();
        }



        // GET: api/entradas/export
        [HttpGet("export")]
        [Authorize]
        public async Task<IActionResult> ExportEntradas()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0) return Unauthorized();

            var entradas = await _db.Entradas
                .Where(e => e.UsuarioId == userId)
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            if (!entradas.Any()) return NotFound("No hay entradas para exportar");

            var exportData = entradas.Select(entrada => new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            }).ToList();

            return File(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exportData)),
                "application/json",
                $"diario_{userId}_{DateTime.Now:yyyyMMdd}.json"
                );
        }




        #endregion




        #region " ADMINISTRADOR. "


        // GET: api/entradas/admin/{id}
        [HttpGet("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<EntradaResponseDTO>> GetEntradaAdmin(int id)
        {
            var entrada = await _db.Entradas
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entrada == null) return NotFound();

            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            };
            return Ok(response);
        }



        // GET: api/entradas/admin/all
        [HttpGet("admin/all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<EntradaResponseDTO>>> GetAllEntradasAdmin()
        {
            var entradas = await _db.Entradas
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            var response = entradas.Select(entrada => new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            });
            return Ok(response);
        }



        // POST: api/entradas/admin
        [HttpPost("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<EntradaResponseDTO>> PostEntradaAdmin([FromBody] EntradaRequestDTO entradaDto)
        {
            var usuario = await _db.Usuarios.FindAsync(entradaDto.UserId);
            if (usuario == null) return BadRequest("Usuario no encontrado");

            var entrada = new Entrada
            {
                UsuarioId = entradaDto.UserId,
                Fecha = entradaDto.Fecha,
                Contenido = entradaDto.Contenido
            };
            _db.Entradas.Add(entrada);

            var hashtags = Regex.Matches(entrada.Contenido, @"#\w+")
                .Select(m => m.Value.Substring(1))
                .Distinct();
            foreach (var tag in hashtags)
            {
                var etiqueta = await _db.Etiquetas.FirstOrDefaultAsync(e => e.Nombre == tag)
                    ?? new Etiqueta { Nombre = tag };
                if (etiqueta.Id == 0) _db.Etiquetas.Add(etiqueta);
                _db.EntradasEtiquetas.Add(new EntradaEtiqueta
                {
                    Entrada = entrada,
                    Etiqueta = etiqueta
                });
            }

            await _db.SaveChangesAsync();

            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = usuario.NombreUsuario,
                Etiquetas = hashtags.ToList()
            };
            return CreatedAtAction(nameof(GetEntradaAdmin), new { id = entrada.Id }, response);
        }




        // PUT: api/entradas/admin/{id}
        [HttpPut("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> PutEntradaAdmin(int id, [FromBody] EntradaRequestDTO entradaDto)
        {
            var entrada = await _db.Entradas
                .Include(e => e.EntradasEtiquetas)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entrada == null) return NotFound();

            entrada.UsuarioId = entradaDto.UserId; // Admin puede cambiar el usuario
            entrada.Fecha = entradaDto.Fecha;
            entrada.Contenido = entradaDto.Contenido;

            _db.EntradasEtiquetas.RemoveRange(entrada.EntradasEtiquetas ?? new List<EntradaEtiqueta>());
            var hashtags = Regex.Matches(entrada.Contenido, @"#\w+")
                .Select(m => m.Value.Substring(1))
                .Distinct();
            foreach (var tag in hashtags)
            {
                var etiqueta = await _db.Etiquetas.FirstOrDefaultAsync(e => e.Nombre == tag)
                    ?? new Etiqueta { Nombre = tag };
                if (etiqueta.Id == 0) _db.Etiquetas.Add(etiqueta);
                _db.EntradasEtiquetas.Add(new EntradaEtiqueta
                {
                    EntradaId = entrada.Id,
                    Etiqueta = etiqueta
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }




        // DELETE: api/entradas/admin/{id}
        [HttpDelete("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteEntradaAdmin(int id)
        {
            var entrada = await _db.Entradas.FindAsync(id);
            if (entrada == null) return NotFound();

            _db.Entradas.Remove(entrada);
            await _db.SaveChangesAsync();
            return NoContent();
        }




        // GET: api/entradas/admin/stats
        [HttpGet("admin/stats")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<GlobalStatsDTO>> GetGlobalStats()
        {
            var entradas = await _db.Entradas
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            var usuarios = await _db.Usuarios.ToListAsync();

            var usuarioMasActivo = entradas
                .GroupBy(e => e.UsuarioId)
                .OrderByDescending(g => g.Count())
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .FirstOrDefault();
            var hashtagMasUsado = entradas
                .SelectMany(e => e.EntradasEtiquetas ?? new List<EntradaEtiqueta>())
                .GroupBy(ee => ee.Etiqueta.Nombre)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Nombre = g.Key, Count = g.Count() })
                .FirstOrDefault();
            var diaMasActivo = entradas
                .GroupBy(e => e.Fecha.Date)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Fecha = g.Key, Count = g.Count() })
                .FirstOrDefault();

            var stats = new GlobalStatsDTO
            {
                TotalEntradas = entradas.Count,
                TotalUsuarios = usuarios.Count,
                UsuarioMasActivo = usuarioMasActivo != null ? _db.Usuarios.Find(usuarioMasActivo.UserId)?.NombreUsuario : null,
                EntradasUsuarioMasActivo = usuarioMasActivo?.Count ?? 0,
                HashtagMasUsado = hashtagMasUsado?.Nombre,
                UsosHashtagMasUsado = hashtagMasUsado?.Count ?? 0,
                DiaMasActivo = diaMasActivo?.Fecha,
                EntradasDiaMasActivo = diaMasActivo?.Count ?? 0
            };
            return Ok(stats);
        }




        // GET: api/entradas/admin/export
        [HttpGet("admin/export")]
        [Authorize(Policy ="AdminOnly")]
        public async Task<IActionResult> ExportAllEntradas()
        {
            var entradas = await _db.Entradas
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            if (!entradas.Any()) return NotFound("No hay entradas para exportar");

            var exportData = entradas.Select(entrada => new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UsuarioId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = entrada.Usuario?.NombreUsuario ?? "",
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new List<string>()
            });


            return File(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize(exportData)),
                "application/json",
                $"diario_completo_{DateTime.Now:yyyyMMdd}, json"
            );
        }






        #endregion



        #region " OTROS. "

        private bool EntradaExists(int idEntrada)
        {
            return _db.Entradas.Any(e => e.Id == idEntrada);
        }


        #endregion


    }
}
