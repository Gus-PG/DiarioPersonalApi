using DiarioPersonalApi.Data;
using DiarioPersonalApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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


        public async Task<ActionResult<string>> Login([FromBody] LoginRequestDTO request)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ContraseñaHash))
                return Unauthorized("Credenciales inválidas");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("mi_clave_secreta_super_segura_123456789"); // Igual que en Program.cs
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }
    }    
}
