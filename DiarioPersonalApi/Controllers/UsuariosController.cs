﻿using DiarioPersonalApi.Data;
using DiarioPersonalApi.Models;
using DiarioPersonalApi.Services;
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
        private readonly EmailService _emailService;

        public UsuariosController(DiarioDbContext db, IConfiguration config, EmailService emailService)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        // POST: api/usuarios/register
        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<string>>> Register(RegisterRequestDTO request)
        {
            // 1 - Validamos si existe el mail en la bbdd.
            if (await _db.Usuarios.AnyAsync(u => u.Email == request.Email))
                return BadRequest("Ya existe un usuario con ese email.");

            // 2 - Creamos nuevo usuario con mail no confirmado.
            var usuario = new Usuario
            {
                Email = request.Email,
                NombreUsuario = request.NombreUsuario, // Alias opcional.
                ContraseñaHash = BCrypt.Net.BCrypt.HashPassword(request.Contraseña),
                Role = "User",
                EmailConfirmed = false                
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            // 3 - Generamos token de confirmación.
            var token = Guid.NewGuid().ToString();
            usuario.ConfirmationToken = token;
            usuario.ConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _db.SaveChangesAsync();

            // 4 - Construimos el enlace de confirmación.
            var confirmLink = $"https://api.diario.peakappstudio.es/api/usuarios/ConfirmarEmail?token={token}";

            // 5 - Enviar correo
            await _emailService.EnviarCorreoConfirmacion(usuario.Email, confirmLink);

            // 6 - Devolver un mensaje
            return Ok(ApiResponse<string>.Ok("Usuario creado correctamente."));            
        }

        [HttpGet("ConfirmarEmail")]
        public async Task<IActionResult> ConfirmarEmail([FromQuery] string token)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.ConfirmationToken == token);
            if (usuario == null) 
                return BadRequest("Token inválido");
            if (usuario.ConfirmationTokenExpiry < DateTime.UtcNow)
                return BadRequest("Token caducado.");

            usuario.EmailConfirmed = true;
            usuario.ConfirmationToken = null;
            usuario.ConfirmationTokenExpiry = null;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Success = true,
                Message = "Correo confirmado. Ya puedes iniciar sesión."
            });                               
        }

        // POST: /api/usuarios/login
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(LoginRequestDTO request)
        {
            // Buscamos usuario por mail.
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Contraseña, usuario.ContraseñaHash))
                return Unauthorized("Credenciales inválidas");


            // (Opcional) Verificar usuario.EmailConfirmed si implementaste confirmación
             if (!usuario.EmailConfirmed)
            {
                return Ok(ApiResponse<LoginResponseDTO>.Fail("Debes confirmar tu correo antes de iniciar sesión."));
            }
             try
            {
                // Generar token JWT.
                var tokenHandler = new JwtSecurityTokenHandler();
                var keyString = _config["Jwt:Key"];
                if (string.IsNullOrWhiteSpace(keyString))
                    return StatusCode(500, "Configuración JWT no encontrada.");

                var key = Encoding.ASCII.GetBytes(keyString);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Role, usuario.Role)
                }),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                // Encapsulamos datos necesarios.
                var loginResponse = new LoginResponseDTO
                {
                    Token = tokenString,
                    Rol = usuario.Role,
                    NombreUsuario = usuario.NombreUsuario
                };

                // Devolver respuesta con token.
                return Ok(ApiResponse<LoginResponseDTO>.Ok(loginResponse, "Login Exitoso"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generando el token: {ex.Message}");
            }

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
                .Where(e => e.UsuarioId == userId)
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


        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<string>>> ForgotPassword(ForgotPasswordRequestDTO request)
        {
            // Lógica para el Forgot Password (Verificaión y envío de contraseña (NO HASH) al mail del usuario) TODO:, NO IMPLEMENTADA**********************************************

            return Ok(ApiResponse<string>.Ok("Correo de recuperación enviado correctamente."));
        }


        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword(ChangePasswordRequestDTO request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return Ok(ApiResponse<string>.Fail("Las nuevas contraseñas no coinciden."));
            }

            // Lógica de cambio de contraseña.   TODO:, NO IMPLEMENTADA**********************************************


            return Ok(ApiResponse<string>.Ok("Contraseña cambiada correctamente."));
        }
    }    
}
