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

namespace DiarioPersonalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntradasController : ControllerBase
    {
        private readonly DiarioDbContext _db;

        public EntradasController(DiarioDbContext db)
        {
            _db = db;
        }

        // GET: api/entradas/usuario/{userId}
        [HttpGet("usuario/{userId}")]
        public async Task<ActionResult<IEnumerable<EntradaResponseDTO>>> GetEntradasUsuario(int userId)
        {
             var entradas = await _db.Entradas
                .Where(e => e.UserId == userId)
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            var response = entradas.Select(entrada => new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UserId,
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
                .ThenInclude (ee => ee.Etiqueta)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entrada == null) return NotFound();

            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UserId,
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
            var entradas = await _db.Entradas
                .Include(e => e.Usuario)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .Where(e => e.EntradasEtiquetas.Any(ee => ee.Etiqueta.Nombre == tag))
                .ToListAsync();
            return Ok(entradas);
        }




        // POST: api/entradas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<EntradaResponseDTO>> PostEntradaUsuario([FromBody] EntradaRequestDTO entradaDTO)
        {
            // Validar que el usuario exista.
            var usuario = await _db.Usuarios.FindAsync(entradaDTO.UserId);
            if (usuario == null) return BadRequest("Usuario no encontrado");

            var entrada = new Entrada
            {
                UserId = entradaDTO.UserId,
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

            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                UserId = entrada.UserId,
                Fecha = entrada.Fecha,
                Contenido = entrada.Contenido,
                NombreUsuario = usuario.NombreUsuario,
                Etiquetas = hashtags.ToList()
            };

            return CreatedAtAction(nameof(GetEntradaUsuario), new { id = entrada.Id }, response);
        }


        // PUT: api/entradas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEntrada(int id, [FromBody] EntradaRequestDTO entradaDTO)
        {
            //if (id != entradaDTO.Id) return BadRequest("ID no coincide"); 
            //var existingEntrada = await _db.Entradas.FindAsync(id);
            //if (existingEntrada == null) return NotFound();
            //if (existingEntrada.UserId != entrada.UserId) return Forbid("No autorizado");

            var entrada = await _db.Entradas
                .Include(e => e.EntradasEtiquetas)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (entrada == null) return NotFound();
            if (entrada.UserId != entradaDTO.UserId) return Forbid("No autorizado");

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

            //_db.Entry(existingEntrada).CurrentValues.SetValues(entrada);

            // Actualizar hashtags (borrar viejos, añadir nuevos)
            //var oldTags = _db.EntradasEtiquetas.Where(ee => ee.EntradaId == id);
            //_db.EntradasEtiquetas.RemoveRange(oldTags);
            //var hashtags = Regex.Matches(entrada.Contenido, @"#\w+")
            //    .Select(m => m.Value.Substring(1))
            //    .Distinct();
            //foreach (var tag in hashtags)
            //{
            //    var etiqueta = await _db.Etiquetas.FirstOrDefaultAsync(e => e.Nombre == tag)
            //        ?? new Etiqueta { Nombre = tag };
            //    if (etiqueta.Id == 0) _db.Etiquetas.Add(etiqueta);
            //    _db.EntradasEtiquetas.Add(new EntradaEtiqueta
            //    {
            //        EntradaId = id,
            //        Etiqueta = etiqueta
            //    });
            //}

            //await _db.SaveChangesAsync();
            //return NoContent();
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

        private bool EntradaExists(int idEntrada)
        {
            return _db.Entradas.Any(e => e.Id == idEntrada);
        }
    }
}
