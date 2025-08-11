using FORO_UTTN_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FORO_UTTN_API.Utils;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IMongoCollection<Posts> _posts;
        private readonly IMongoCollection<Users> _users;
        private readonly IMongoCollection<Responses> _responses;
        private readonly IMongoCollection<Actions> _actions;
        private readonly MongoService _mongoService;

        public PostsController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _posts = mongoService.Posts;
            _users = mongoService.Users;
            _responses = mongoService.Responses;
            _actions = mongoService.Actions;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] string? post_id, [FromQuery] bool? verified)
        {
            try
            {
                if (!string.IsNullOrEmpty(post_id))
                {
                    var post = await _posts.Find(p => p.Id == post_id).FirstOrDefaultAsync();
                    if (post == null)
                    {
                        return NotFound(new { success = false, message = "Post no encontrado" });
                    }

                    var user = await _users.Find(u => u.Id == post.UsuarioId).FirstOrDefaultAsync();
                    var date = post.FechaPublicacion; // Ya es DateTime, no necesita conversión
                    var responsesCount = await _responses.CountDocumentsAsync(r => r.PreguntaId == post.Id);

                    return Ok(new
                    {
                        post_id = post.Id,
                        apodo = user?.Apodo ?? "Desconocido",
                        titulo = post.Titulo,
                        contenido = post.Contenido,
                        pub_date = DateUtils.DateMX(date),
                        pub_time = DateUtils.TimeMX(date),
                        respuestas = responsesCount,
                        mensaje_admin = post.MensajeAdmin
                    });
                }

                var query = Builders<Posts>.Filter.Empty;
                if (verified.HasValue)
                {
                    query = Builders<Posts>.Filter.Eq(p => p.Verified, verified.Value);
                }

                var posts = await _posts.Find(query).ToListAsync();
                var postDetails = new List<object>();

                foreach (var post in posts)
                {
                    var user = await _users.Find(u => u.Id == post.UsuarioId).FirstOrDefaultAsync();
                    var date = post.FechaPublicacion; // Ya es DateTime, no necesita conversión
                    var responsesCount = await _responses.CountDocumentsAsync(r => r.PreguntaId == post.Id);

                    postDetails.Add(new
                    {
                        post_id = post.Id,
                        apodo = user?.Apodo ?? "Desconocido",
                        titulo = post.Titulo,
                        contenido = post.Contenido,
                        pub_date = DateUtils.DateMX(date),
                        pub_time = DateUtils.TimeMX(date),
                        respuestas = responsesCount,
                        mensaje_admin = post.MensajeAdmin
                    });
                }

                return Ok(postDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Posts newPost)
        {
            try
            {
                newPost.FechaPublicacion = DateTime.UtcNow;

                await _posts.InsertOneAsync(newPost);
                await RegistrarAccion(newPost.UsuarioId, 1, "Creó una publicación", newPost.Id, ObjectiveType.Post);

                return CreatedAtAction(nameof(GetPosts), new { post_id = newPost.Id }, new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] Posts updateData)
        {
            try
            {
                var update = Builders<Posts>.Update
                    .Set(p => p.Titulo, updateData.Titulo)
                    .Set(p => p.Contenido, updateData.Contenido)
                    .Set(p => p.Modified, true);

                var result = await _posts.UpdateOneAsync(p => p.Id == id, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                await RegistrarAccion(updateData.UsuarioId, 2, "Modificó su publicación", id, ObjectiveType.Post);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id, [FromBody] DeletePostRequest request)
        {
            try
            {
                var result = await _posts.DeleteOneAsync(p => p.Id == id);
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                await RegistrarAccion(request.UsuarioId, 3, "Eliminó su publicación", id, ObjectiveType.Post);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Método auxiliar para registrar acciones
        private async Task RegistrarAccion(string userId, int actionType, string details, string objectiveId, ObjectiveType objectiveType)
        {
            var action = new Actions
            {
                UserId = userId,
                ActionType = actionType,
                Details = details,
                ActionDate = DateTime.UtcNow,
                ObjectiveId = objectiveId,
                ObjectiveType = objectiveType
            };

            await _actions.InsertOneAsync(action);
        }
    }

    // Clase auxiliar para el request del DELETE
    public class DeletePostRequest
    {
        public string UsuarioId { get; set; } = string.Empty;
    }
}

