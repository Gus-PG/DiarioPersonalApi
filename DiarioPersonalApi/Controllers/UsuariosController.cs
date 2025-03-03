using DiarioPersonalApi.Data;
using DiarioPersonalApi.Models;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IConfiguration _config;

        public UsuariosController(DiarioDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
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


        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequestDTO request)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == request.NombreUsuario);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ContraseñaHash))
                return Unauthorized("Credenciales inválidas");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.NombreUsuario == "user2" ? "Admin" : "User") // Temporal OJO*****************************
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }



        // GET: api/usuarios/{userId}/stats
        [HttpGet("{userId}/stats")]
        [Authorize]
        public async Task<ActionResult<UsuarioStatsDTO>> GetUsuarioStats(int userId)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (currentUserId != userId && !User.IsInRole("Admin")) return Forbid("No autorizado");

            var usuario = await _db.Usuarios.FindAsync(userId);
            if (usuario == null) return NotFound("Usuario no encontrado");

            var entradas = await _db.Entradas
                .Where(e => e.UserId == userId)
                .Include(e => e.EntradasEtiquetas)
                .ThenInclude(ee => ee.Etiqueta)
                .ToListAsync();

            if (!entradas.Any()) return NotFound("No se encontraron entradas para este usuario");

            var tags = entradas
                .SelectMany(e => e.EntradasEtiquetas ?? new List<EntradaEtiqueta>())
                .GroupBy(ee => ee.Etiqueta.Nombre)
                .ToDictionary(g => g.Key, g => g.Count());

            var diaMasActivo = entradas
                .GroupBy(e => e.Fecha.Date)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            var stats = new UsuarioStatsDTO
            {
                TotalEntradas = entradas.Count,
                TotalEtiquetas = tags.Count,
                EtiquetasMasUsadas = tags.OrderByDescending(t => t.Value).Take(5).ToDictionary(t => t.Key, t => t.Value),
                DiaMasActivo = diaMasActivo == default ? null : diaMasActivo
            };
            return Ok(stats);
        }
    }    
}
