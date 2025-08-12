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
                var update = Builders<Users>.Update
                    .Set(u => u.Apodo, updateData.Apodo)
                    .Set(u => u.Email, updateData.Email)
                    .Set(u => u.Admin, updateData.Admin);

                var result = await _users.UpdateOneAsync(u => u.Id == id, update);

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