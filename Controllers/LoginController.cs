using FORO_UTTN_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using MongoDB.Driver;
using FORO_UTTN_API.DTOs;
using LoginRequest = FORO_UTTN_API.DTOs.LoginRequest;


namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IMongoCollection<SignUp> _signupCollection;
        private readonly IMongoCollection<Login> _loginCollection;

        public LoginController(MongoService mongoService)
        {
            _signupCollection = mongoService.Signups;
            _loginCollection = mongoService.Logins;
        }
        //POST /login Inicia sesión de un usuario existente.

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _signupCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (user == null)
                return Ok(new { success = false, message = "Usuario no encontrado" });

            bool isMatch = BCrypt.Net.BCrypt.Verify(request.Contraseña, user.Contraseña);
            if (!isMatch)
                return Ok(new { success = false, message = "Contraseña incorrecta" });

            var log = new Login
            {
                Email = user.Email,
                ContraseñaHash = user.Contraseña,
                FechaInicioSesion = DateTime.UtcNow,
                Token = "N/A"
            };

            await _loginCollection.InsertOneAsync(log);

            return Ok(new
            {
                success = true,
                message = "Inicio de sesión exitoso",
                user = new { id = user.Id, email = user.Email }
            });
        }
    }
}
