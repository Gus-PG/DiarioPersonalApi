using DiarioPersonalApi.Data;
using DiarioPersonalApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiarioPersonalApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DiarioDbContext _db;

        public UsuariosController(DiarioDbContext db)
        {
            _db = db;
        }

        // POST: api/usuarios/register
        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> Register([FromBody] RegisterRequestDTO request)
        {
            if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == request.NombreUsuario))
                return BadRequest("Usuario ya existe");

            var usuario = new Usuario
            {
                NombreUsuario = request.NombreUsuario,
                ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(request.Contraseña)
            };
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Register), new { id = usuario.Id }, usuario);
        }
    }
}
