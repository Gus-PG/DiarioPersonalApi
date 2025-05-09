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
using DiarioPersonalApi.Data.Repositories;

namespace DiarioPersonalApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    
    [Authorize] // Requiere token para todos los métodos. Inyecta 'User'.
    public class EntradasController : ControllerBase
    {
        private readonly IEntradaRepository _iRepo;

        public EntradasController(IEntradaRepository iRepo)
        {
            _iRepo = iRepo;
        }

        // Variables de identificación y autorización.
        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value;



        // 1) Obtener todas las entradas (User y Admin)
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<EntradaResponseDTO>>>> ObtenerEntradas()
        {
            var userId = GetUserId();
            var role = GetRole();

            IEnumerable<Entrada> entradas;

            if (role == "Admin")
                entradas = await _iRepo.GetAllAsync();
            else
                entradas = await _iRepo.GetByUserIdAsync(userId);

            var response = entradas.Select(e => new EntradaResponseDTO
            {
                Id = e.Id,
                Contenido = e.Contenido,
                Fecha = e.Fecha,
                Etiquetas = e.EntradasEtiquetas.Select(ee => ee.Etiqueta.Nombre).ToList()
            });

            return Ok(ApiResponse<IEnumerable<EntradaResponseDTO>>.Ok(response, "Entradas obtenidas"));
        }


        // 2) Obtener una entrada por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EntradaResponseDTO>>> ObtenerEntrada(int id)
        {
            var userId = GetUserId();
            var role = GetRole();
            var entrada = await _iRepo.GetByIdAsync(id);

            if (entrada == null)
                return Ok(ApiResponse<EntradaResponseDTO>.Fail("Entrada no encontrada"));

            if (entrada.UsuarioId != userId && role != "Admin")
                return Forbid();

            var response = new EntradaResponseDTO
            {
                Id = entrada.Id,
                Contenido = entrada.Contenido,
                Fecha = entrada.Fecha,
                UserId = entrada.UsuarioId,
                Etiquetas = entrada.EntradasEtiquetas?.Select(ee => ee.Etiqueta.Nombre).ToList() ?? new()
            };

            return Ok(ApiResponse<EntradaResponseDTO>.Ok(response, "Entrada encontrada"));
        }


        // 3) Obtiene preview de las entradas (usando paginación (20 por página)).
        [HttpGet("preview")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<EntradaPreviewDTO>>>> GetEntradasPreview([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _iRepo.GetPreviewEntradasPaginadoAsync(userId, page, pageSize);
            return Ok(ApiResponse<List<EntradaPreviewDTO>>.Ok(result));
        }


        // 4) Crear una entrada.
        [HttpPost]
        public async Task<ActionResult<ApiResponse<string>>> CrearEntrada(EntradaRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Contenido))            
                return Ok(ApiResponse<string>.Fail("Entrada no puede estar vacía."));
            
            if (request.Contenido.Length > 5000)
                return Ok(ApiResponse<string>.Fail("Entrada excede límite permitido de 5000 caracteres."));

            var userId = GetUserId();
            var entrada = new Entrada 
            { 
                Contenido = request.Contenido,
                Fecha = request.Fecha,
                UsuarioId = userId

            };

            await _iRepo.AddAsync(entrada);
            await _iRepo.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Entrada creada correctamente"));
        }


        // 5) Editar entrada
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> EditarEntrada(int id, [FromBody] EntradaRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.Contenido))
                return Ok(ApiResponse<string>.Fail("El contenido no puede estar vacío."));

            if (request.Contenido.Length > 5000)
                return Ok(ApiResponse<string>.Fail("El contenido supera el límite de 5000 caracteres."));

            var userId = GetUserId();
            var userRole = GetRole();

            var entrada = await _iRepo.GetByIdAsync(id);
            if (entrada == null)
                return NotFound(ApiResponse<string>.Fail("La entrada no existe."));

            // Solo el autor o un admin pueden editar
            if (entrada.UsuarioId != userId && userRole != "Admin")
                return Forbid();

            entrada.Contenido = request.Contenido;
            entrada.Fecha = request.Fecha;

            //// ACTUALIZACIÓN ETIQUETAS (opcional, si las gestionas también desde aquí)
            //if (request.Etiquetas != null)
            //{
            //    // Elimina las actuales
            //    _context.EntradasEtiquetas.RemoveRange(entrada.Etiquetas);

            //    // Crea nuevas
            //    entrada.Etiquetas = request.Etiquetas.Select(nombre => new EntradaEtiqueta
            //    {
            //        Etiqueta = new Etiqueta { Nombre = nombre }, // Asegúrate de no duplicar etiquetas aquí
            //        EntradaId = entrada.Id
            //    }).ToList();
            //}

            _iRepo.Update(entrada);
            await _iRepo.SaveChangesAsync();

            //await _iRepo.UpdateAsync(entrada);

            return Ok(ApiResponse<string>.Ok("Entrada actualizada correctamente"));
        }


        // 6) Eliminar entrada
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> EliminarEntrada(int id)
        {
            var userId = GetUserId();
            var role = GetRole();

            var entrada = await _iRepo.GetByIdAsync(id);
            if (entrada == null)
                return NotFound(ApiResponse<string>.Fail("Entrada no encontrada"));

            // Solo el autor o un admin pueden eliminar
            if (entrada.UsuarioId != userId && role != "Admin")
                return Forbid();

            _iRepo.Delete(entrada);
            await _iRepo.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Entrada eliminada correctamente"));
        }


        // 7) Endpoint extra: estadísticas solo para Admin
        [HttpGet("stats")]
        public IActionResult GetGlobalStats()
        {
            if (GetRole() != "Admin")
                return Forbid();

            // Simulación de estadísticas TODO: ELIMINAR SIMULACIÓN POR DATOS REALES (HABRÄ QUE AJUSTAR MËTODO PARA CONTEO NO DUOLICADO)**********
            var stats = new { TotalEntradas = 100, UsuariosActivos = 20 };
            return Ok(ApiResponse<object>.Ok(stats, "Estadísticas globales"));
        }


        // 8) Buscar por Hashtag
        [HttpGet("buscar")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EntradaResponseDTO>>>> BuscarPorHashtag([FromQuery] string hashtag)
        {
            if (string.IsNullOrWhiteSpace(hashtag))
                return Ok(ApiResponse<IEnumerable<EntradaResponseDTO>>.Fail("Hashtag inválido"));

            var userId = GetUserId();
            var role = GetRole();

            IEnumerable<Entrada> entradas;

            if (role == "Admin")
                entradas = await _iRepo.SearchByHashtagAdminAsync(hashtag);
            else
                entradas = await _iRepo.SearchByHashtagAsync(userId, hashtag);

            var response = entradas.Select(e => new EntradaResponseDTO
            {
                Id = e.Id,
                Contenido = e.Contenido,
                Fecha = e.Fecha,
                Etiquetas = e.EntradasEtiquetas.Select(ee => ee.Etiqueta.Nombre).ToList()
            });

            return Ok(ApiResponse<IEnumerable<EntradaResponseDTO>>.Ok(response, $"Entradas encontradas con #{hashtag}"));
        }

        [HttpGet("etiquetas-usuario")]
        public async Task<ActionResult<ApiResponse<List<string>>>> ObtenerEtiquetasUsuario()
        {
            var userId = GetUserId();

            var etiquetas = await _iRepo.GetEtiquetasUsuarioAsync(userId);

            return Ok(ApiResponse<List<string>>.Ok(etiquetas));
        }

        [HttpPost("buscar-avanzado")]
        public async Task<ActionResult<ApiResponse<IEnumerable<EntradaPreviewDTO>>>> BuscarPorTextoYEtiquetas([FromBody] FiltroBusquedaDTO filtro)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (string.IsNullOrWhiteSpace(filtro.Texto) && (filtro.Etiquetas == null || !filtro.Etiquetas.Any()))
                return BadRequest(ApiResponse<IEnumerable<EntradaPreviewDTO>>.Fail("Debe especificar al menos un texto o una etiqueta"));

            var entradas = await _iRepo.SearchByTextoYOEtiquetasAsync(
                    filtro.Texto,
                    filtro.Etiquetas,
                    role == "Admin" ? null : userId);

            var resultado = entradas.Select(e => new EntradaPreviewDTO
            {
                Id = e.Id,
                Fecha = e.Fecha,
                Preview = e.Contenido.Length > 200 ? e.Contenido.Substring(0, 200) + "..." : e.Contenido,
                Etiquetas = e.EntradasEtiquetas.Select(ee => ee.Etiqueta.Nombre).ToList()
            });

            return Ok(ApiResponse<IEnumerable<EntradaPreviewDTO>>.Ok(resultado));
        }


        // 9) Exportar entradas
        [HttpGet("export")]
        public async Task<ActionResult<ApiResponse<string>>> ExportEntradas([FromQuery] int? id = null)
        {
            var userId = GetUserId();
            var role = GetRole();

            List<Entrada> entradas;

            if (id.HasValue)
            {
                var entrada = await _iRepo.GetByIdAsync(id.Value);
                if (entrada == null)
                    return Ok(ApiResponse<string>.Fail("Entrada no encontrada"));

                if (entrada.UsuarioId != userId && role != "Admin")
                    return Forbid();

                entradas = new List<Entrada> { entrada };
            }
            else
            {
                entradas = role == "Admin"
                    ? (await _iRepo.GetAllAsync()).ToList()
                    : (await _iRepo.GetByUserIdAsync(userId)).ToList();
            }

            // 🔵 Aquí exportamos a CSV/JSON o lo que decidas, simulo export: ****** TODO: A QUÉ EXPORTAMOS?? ********************************
            var export = $"Export de {entradas.Count} entrada(s) realizada correctamente";

            return Ok(ApiResponse<string>.Ok(export));
        }


        [HttpPost("exportar-filtrado")]
        public async Task<IActionResult> ExportarFiltrado([FromBody] FiltroBusquedaDTO filtro)
        {
            var userId = GetUserId();
            var role = GetRole();

            if (string.IsNullOrWhiteSpace(filtro.Texto) && (filtro.Etiquetas == null || !filtro.Etiquetas.Any()))
                return BadRequest(ApiResponse<string>.Fail("Debe especificar al menos un texto o una etiqueta."));

            var entradas = await _iRepo.SearchByTextoYOEtiquetasAsync(
                filtro.Texto,
                filtro.Etiquetas,
                role == "Admin" ? null : userId);

            var textoPlano = new StringBuilder();
            foreach (var entrada in entradas.OrderByDescending(e => e.Fecha))
            {
                textoPlano.AppendLine($"📅 {entrada.Fecha:dd/MM/yyyy}");
                textoPlano.AppendLine(entrada.Contenido);
                textoPlano.AppendLine("------------------------------------------------------------");
            }

            var fileName = $"EntradasExportadas_{DateTime.Now:yyyyMMddHHmmss}.txt";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            await System.IO.File.WriteAllTextAsync(filePath, textoPlano.ToString());

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, "text/plain", fileName);
        }

    }
}
