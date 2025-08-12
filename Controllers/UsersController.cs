using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FORO_UTTN_API.Models;
using FORO_UTTN_API.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<Response> _responses;
        private readonly MongoService _mongoService;

        public UsersController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _users = mongoService.Users;
            _posts = mongoService.Posts;
            _responses = mongoService.Responses;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _users.Find(_ => true).ToListAsync();
                var formatted = users.Select(u => new
                {
                    user_id = u.Id,
                    user_email = u.Email,
                    apodo = u.Apodo,
                    admin = u.Admin,
                    foto_perfil = u.Perfil?.FotoPerfil ?? "0"
                });
                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

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
                var date = user.FechaRegistro ?? DateTime.UtcNow;
                var result = new
                {
                    _id = user.Id,
                    email = user.Email,
                    regDate = DateUtils.DateMX(date),
                    regTime = DateUtils.TimeMX(date),
                    apodo = user.Apodo,
                    admin = user.Admin,
                    perfil = user.Perfil == null ? null : new
                    {
                        biografia = user.Perfil.Biografia,
                        foto_perfil = user.Perfil.FotoPerfil
                    }
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{userId}/posts")]
        public async Task<IActionResult> GetUserPosts(string userId)
        {
            try
            {
                var posts = await _posts.Find(p => p.UsuarioId == userId).ToListAsync();
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                var formatted = posts.Select(post =>
                {
                    var date = post.FechaPublicacion ?? DateTime.UtcNow;
                    return new
                    {
                        post_id = post.Id,
                        apodo = user?.Apodo ?? "Desconocido",
                        titulo = post.Titulo,
                        contenido = post.Contenido,
                        pub_date = DateUtils.DateMX(date),
                        pub_time = DateUtils.TimeMX(date),
                        respuestas = post.Respuestas?.Count ?? 0,
                        mensaje_admin = post.MensajeAdmin
                    };
                });
                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{userId}/answered-posts")]
        public async Task<IActionResult> GetUserResponses(string userId)
        {
            try
            {
                var responses = await _responses.Find(r => r.UsuarioId == userId).ToListAsync();
                var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                var formatted = responses.Select(resp =>
                {
                    var date = resp.FechaRespuesta;
                    return new
                    {
                        pregunta_id = resp.PreguntaId,
                        respuesta_id = resp.Id,
                        contenido = resp.Contenido,
                        res_date = DateUtils.DateMX(date),
                        res_time = DateUtils.TimeMX(date),
                        usuario = new
                        {
                            id = user?.Id,
                            apodo = user?.Apodo ?? "Desconocido",
                            foto_perfil = user?.Perfil?.FotoPerfil
                        }
                    };
                });
                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User update)
        {
            try
            {
                update.Id = id;
                var result = await _users.ReplaceOneAsync(u => u.Id == id, update);
                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Usuario no encontrado" });
                }
                await ActionLogger.RegistrarAccion(_mongoService, id, 10, "Editó su perfil", id, "User");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

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
                await ActionLogger.RegistrarAccion(_mongoService, id, 23, "Eliminó su cuenta", id, "User");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }
}