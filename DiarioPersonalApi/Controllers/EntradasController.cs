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

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        private string GetRole() => User.FindFirst(ClaimTypes.Role).Value;

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
                Etiquetas = entrada.EntradasEtiquetas.Select(ee => ee.Etiqueta.Nombre).ToList()
            };

            return Ok(ApiResponse<EntradaResponseDTO>.Ok(response, "Entrada encontrada"));
        }

        //3 Crear una entrada.
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


        // 4) Editar entrada
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> EditarEntrada(int id, EntradaRequestDTO request)
        {
            var userId = GetUserId();
            var role = GetRole();
            var entrada = await _iRepo.GetByIdAsync(id);

            if (entrada == null)
                return Ok(ApiResponse<string>.Fail("Entrada no encontrada"));

            if (entrada.UsuarioId != userId && role != "Admin")
                return Forbid();

            entrada.Contenido = request.Contenido;
            entrada.Fecha = request.Fecha;
            _iRepo.Update(entrada);
            await _iRepo.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Entrada actualizada correctamente"));
        }


        // 5) Eliminar entrada
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<string>>> EliminarEntrada(int id)
        {
            var userId = GetUserId();
            var role = GetRole();
            var entrada = await _iRepo.GetByIdAsync(id);

            if (entrada == null)
                return Ok(ApiResponse<string>.Fail("Entrada no encontrada"));

            if (entrada.UsuarioId != userId && role != "Admin")
                return Forbid();

            _iRepo.Delete(entrada);
            await _iRepo.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok("Entrada eliminada correctamente"));
        }


        // 6) Endpoint extra: estadísticas solo para Admin
        [HttpGet("stats")]
        public IActionResult GetGlobalStats()
        {
            if (GetRole() != "Admin")
                return Forbid();

            // Simulación de estadísticas TODO: ELIMINAR SIMULACIÓN POR DATOS REALES (HABRÄ QUE AJUSTAR MËTODO PARA CONTEO NO DUOLICADO)**********
            var stats = new { TotalEntradas = 100, UsuariosActivos = 20 };
            return Ok(ApiResponse<object>.Ok(stats, "Estadísticas globales"));
        }

    }
}
