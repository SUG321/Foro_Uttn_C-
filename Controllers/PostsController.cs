using FORO_UTTN_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly IMongoCollection<Posts> _posts;
        private readonly IMongoCollection<Users> _users;

        public PostsController(MongoService mongoService)
        {
            _posts = mongoService.Posts;
            _users = mongoService.Users;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _posts.Find(_ => true).ToListAsync();
                var result = new List<object>();

                foreach (var post in posts)
                {
                    var user = await _users.Find(u => u.Id == post.UsuarioId).FirstOrDefaultAsync();

                    result.Add(new
                    {
                        post_id = post.Id,
                        apodo = user?.Apodo ?? "Desconocido",
                        titulo = post.Titulo,
                        contenido = post.Contenido,
                        pub_date = post.FechaPublicacion.ToString("yy/MM/dd"),
                        respuestas = post.Respuestas?.Count ?? 0
                    });
                }

                return Ok(result);
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
                    return NotFound(new { success = false, message = "Post no encontrado" });

                var user = await _users.Find(u => u.Id == post.UsuarioId).FirstOrDefaultAsync();

                return Ok(new
                {
                    post_id = post.Id,
                    apodo = user?.Apodo ?? "Desconocido",
                    titulo = post.Titulo,
                    contenido = post.Contenido,
                    pub_date = post.FechaPublicacion.ToString("yy/MM/dd"),
                    respuestas = post.Respuestas?.Count ?? 0
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] Posts post)
        {
            try
            {
                post.FechaPublicacion = DateTime.UtcNow;
                post.Respuestas = new List<string>();
                post.Votos = 0;
                post.Modified = false;

                await _posts.InsertOneAsync(post);
                return StatusCode(201, new { success = true, message = "Post creado correctamente", post_id = post.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(string id, [FromBody] Posts updatedData)
        {
            try
            {
                var update = Builders<Posts>.Update
                    .Set(p => p.Titulo, updatedData.Titulo)
                    .Set(p => p.Contenido, updatedData.Contenido)
                    .Set(p => p.CategoriaId, updatedData.CategoriaId)
                    .Set(p => p.Modified, true);

                var result = await _posts.UpdateOneAsync(p => p.Id == id, update);

                if (result.MatchedCount == 0)
                    return NotFound(new { success = false, message = "Post no encontrado" });

                return Ok(new { success = true, message = "Post actualizado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id)
        {
            try
            {
                var result = await _posts.DeleteOneAsync(p => p.Id == id);

                if (result.DeletedCount == 0)
                    return NotFound(new { success = false, message = "Post no encontrado" });

                return Ok(new { success = true, message = "Post eliminado" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}

