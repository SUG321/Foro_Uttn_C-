using FORO_UTTN_API.Models;
using FORO_UTTN_API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<Users> _users;
        private readonly IMongoCollection<Posts> _posts;
        private readonly IMongoCollection<Responses> _responses;
        private readonly MongoService _mongoService;

        public UsersController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _users = mongoService.Users;
            _posts = mongoService.Posts;
            _responses = mongoService.Responses;
        }

        // Obtener todas las preguntas realizadas por un usuario
        [HttpGet("{userId}/posts")]
        public async Task<IActionResult> GetPostsByUserId(string userId)
        {
            try
            {
                var posts = await _posts.Find(p => p.UsuarioId == userId).ToListAsync();
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                var postDetails = posts.Select(post => new
                {
                    post_id = post.Id,
                    apodo = user?.Apodo ?? "Desconocido",
                    titulo = post.Titulo,
                    contenido = post.Contenido,
                    pub_date = DateUtils.DateMX(post.FechaPublicacion),
                    pub_time = DateUtils.TimeMX(post.FechaPublicacion),
                    respuestas = post.Respuestas?.Count ?? 0,
                    mensaje_admin = post.MensajeAdmin
                }).ToList();

                return Ok(postDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Obtener todas las respuestas de un usuario
        [HttpGet("{userId}/answered-posts")]
        public async Task<IActionResult> GetAnsweredPostsByUserId(string userId)
        {
            try
            {
                var responses = await _responses.Find(r => r.UsuarioId == userId).ToListAsync();
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

                var responseDetails = responses.Select(resp => new
                {
                    pregunta_id = resp.PreguntaId,
                    respuesta_id = resp.Id,
                    contenido = resp.Contenido,
                    res_date = DateUtils.DateMX(resp.FechaRespuesta),
                    res_time = DateUtils.TimeMX(resp.FechaRespuesta),
                    votos = resp.Votos.Count, // Contamos los votos
                    usuario = new
                    {
                        id = user?.Id,
                        apodo = user?.Apodo ?? "Desconocido",
                    }
                }).ToList();

                return Ok(responseDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Listar todos los usuarios
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _users.Find(u => true).ToListAsync();

                var formUsers = users.Select(user => new
                {
                    user_id = user.Id,
                    apodo = user.Apodo,
                    admin = user.Admin,
                }).ToList();

                return Ok(formUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Obtener un usuario por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            try
            {
                var user = await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                var response = new
                {
                    _id = user.Id,
                    email = user.Email,
                    regDate = DateUtils.DateMX(user.FechaRegistro),
                    regTime = DateUtils.TimeMX(user.FechaRegistro),
                    apodo = user.Apodo,
                    admin = user.Admin
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Actualizar un usuario existente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] Users updateData)
        {
            try
            {
                var update = Builders<Users>.Update
                    .Set(u => u.Apodo, updateData.Apodo)
                    .Set(u => u.Email, updateData.Email)
                    .Set(u => u.Admin, updateData.Admin);

                var result = await _users.UpdateOneAsync(u => u.Id == id, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eliminar un usuario
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var result = await _users.DeleteOneAsync(u => u.Id == id);

                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }

                return Ok(new { success = true, message = "Usuario eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
