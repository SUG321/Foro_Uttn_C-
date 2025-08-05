using FORO_UTTN_API.DTOs;
using FORO_UTTN_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly IMongoCollection<SignUp> _signupCollection;

        public SignUpController(MongoService mongoService)
        {
            _signupCollection = mongoService.Signups;
        }

        //POST /register Registra un nuevo usuario.
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Validar formato de email
            if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return BadRequest(new { success = false, message = "Correo electrónico inválido" });

            if (!request.Email.EndsWith("@uttn.mx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { success = false, message = "Solo se permiten correos @uttn.mx" });

            if (request.Contraseña != request.ConfirmacionContraseña)
                return BadRequest(new { success = false, message = "Las contraseñas no coinciden" });

            var existingUser = await _signupCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (existingUser != null)
                return BadRequest(new { success = false, message = "El email ya está registrado" });

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Contraseña);

            var newUser = new SignUp
            {
                NombreCompleto = request.NombreCompleto,
                Email = request.Email,
                Contraseña = hashedPassword,
                ConfirmacionContraseña = hashedPassword
            };

            await _signupCollection.InsertOneAsync(newUser);

            return Ok(new
            {
                success = true,
                message = "Usuario registrado exitosamente",
                user = new { id = newUser.Id }
            });
        }
    }
}
