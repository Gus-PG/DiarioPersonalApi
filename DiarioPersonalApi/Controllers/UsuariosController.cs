using DiarioPersonalApi.Data;
using DiarioPersonalApi.Models;
using Microsoft.AspNetCore.Http;
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
        public async Task<ActionResult<Usuario>> Register(Usuario usuario)
        {
            if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == usuario.NombreUsuario))
                return BadRequest("Usuario ya existe");
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Register), new { id = usuario.Id }, usuario);
        }
    }
}
