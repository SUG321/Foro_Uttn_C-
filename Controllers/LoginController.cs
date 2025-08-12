using FORO_UTTN_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using FORO_UTTN_API.DTOs;
using BCrypt.Net;
using FORO_UTTN_API.Utils;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Models.Action> _actions;
        private readonly MongoService _mongoService;

        public LoginController(MongoService mongoService)
        {
            _userCollection = mongoService.Users;
            _mongoService = mongoService;
            _actions = mongoService.Actions;
        }

        // POST /login Inicia sesión de un usuario existente.
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // Buscar al usuario por correo
            var user = await _userCollection.Find(u => u.Email == request.Email).FirstOrDefaultAsync();
            if (user == null)
            {
                return Ok(new { success = false, message = "Usuario no encontrado" });
            }

            // Verificar que la contraseña sea correcta
            bool isMatch = BCrypt.Net.BCrypt.Verify(request.Contraseña, user.Contraseña);
            if (!isMatch)
            {
                return Ok(new { success = false, message = "Contraseña incorrecta" });
            }

            // Registrar la acción de inicio de sesión
            await RegistrarAccion(user.Id, 1, "Creó una publicación");

            // Retornar la respuesta con los datos del usuario
            return Ok(new
            {
                success = true,
                user = new
                {
                    id = user.Id,
                    apodo = user.Apodo,
                    email = user.Email,
                    admin = user.Admin
                },
            });
        }

        // Método auxiliar para registrar acciones
        private async Task RegistrarAccion(string userId, int actionType, string details)
        {
            var action = new Models.Action
            {
                UserId = userId,
                ActionType = actionType,
                Details = details,
                ActionDate = DateTime.UtcNow
            };

            await _mongoService.Actions.InsertOneAsync(action);
        }

    }
}
