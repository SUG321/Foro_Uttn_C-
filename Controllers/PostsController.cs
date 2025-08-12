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
    public class PostsController : ControllerBase
    {
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Response> _responses;
        private readonly MongoService _mongoService;

        public PostsController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _posts = mongoService.Posts;
            _users = mongoService.Users;
            _responses = mongoService.Responses;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _posts.Find(_ => true).ToListAsync();
                var formatted = new List<object>();
                foreach (var post in posts)
                {
                    var user = await _users.Find(u => u.Id == post.UsuarioId).FirstOrDefaultAsync();
                    var count = await _responses.CountDocumentsAsync(r => r.PreguntaId == post.Id);
                    var date = post.FechaPublicacion ?? DateTime.UtcNow;
                    formatted.Add(new
                    {
                        post_id = post.Id,
                        user_id = post.UsuarioId,
                        apodo = user?.Apodo ?? "Desconocido",
                        titulo = post.Titulo,
                        contenido = post.Contenido,
                        pub_date = DateUtils.DateMX(date),
                        pub_time = DateUtils.TimeMX(date),
                        respuestas = count,
                        mensaje_admin = post.MensajeAdmin
                    });
                }
                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(string id)
        {
            try
            {
                var post = await _posts.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }
                var user = await _users.Find(u => u.Id == post.UsuarioId).FirstOrDefaultAsync();
                var count = await _responses.CountDocumentsAsync(r => r.PreguntaId == post.Id);
                var date = post.FechaPublicacion ?? DateTime.UtcNow;
                var result = new
                {
                    post_id = post.Id,
                    user_id = post.UsuarioId,
                    apodo = user?.Apodo ?? "Desconocido",
                    titulo = post.Titulo,
                    contenido = post.Contenido,
                    pub_date = DateUtils.DateMX(date),
                    pub_time = DateUtils.TimeMX(date),
                    respuestas = count,
                    mensaje_admin = post.MensajeAdmin
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Post newPost)
        {
            try
            {
                newPost.FechaPublicacion = DateTime.UtcNow;
                await _posts.InsertOneAsync(newPost);
                await ActionLogger.RegistrarAccion(_mongoService, newPost.UsuarioId, 1, "Creó una publicación", newPost.Id, "Post");
                return StatusCode(201, new { success = true, post_id = newPost.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] Post updatePost)
        {
            try
            {
                updatePost.Modified = true;
                var result = await _posts.ReplaceOneAsync(p => p.Id == id, updatePost);
                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }
                await ActionLogger.RegistrarAccion(_mongoService, updatePost.UsuarioId, 2, "Modificó su publicación", id, "Post");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id, [FromBody] dynamic body)
        {
            try
            {
                var result = await _posts.DeleteOneAsync(p => p.Id == id);
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }
                await ActionLogger.RegistrarAccion(_mongoService, body.usuario_id.ToString(), 3, "Eliminó su publicación", id, "Post");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }
}