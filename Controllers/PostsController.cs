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
    public class PostsController(MongoService mongoService) : ControllerBase
    {
        private readonly IMongoCollection<Post> _posts = mongoService.Posts;
        private readonly IMongoCollection<User> _users = mongoService.Users;
        private readonly IMongoCollection<Response> _responses = mongoService.Responses;
        private readonly MongoService _mongoService = mongoService;

        [HttpGet]
        public async Task<IActionResult> GetAllPosts([FromQuery] string post_id = null, [FromQuery] string verified = null)
        {
            try
            {
                // Si se proporciona un post_id, buscamos ese post
                if (!string.IsNullOrEmpty(post_id))
                {
                    var post = await _posts.Find(p => p.Id == post_id).FirstOrDefaultAsync();
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

                // Si no se pasa post_id, verificamos el parámetro 'verified'
                var query = Builders<Post>.Filter.Empty;

                if (!string.IsNullOrEmpty(verified))
                {
                    if (verified.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = Builders<Post>.Filter.Eq(p => p.Verified, true);
                    }
                    else if (verified.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                    {
                        query = Builders<Post>.Filter.Eq(p => p.Verified, false);
                    }
                    else
                    {
                        return BadRequest(new { success = false, message = "Parámetro 'verified' debe ser 'true' o 'false'" });
                    }
                }
                else
                {
                    query = Builders<Post>.Filter.Eq(p => p.Verified, true); // Si no se pasa 'verified', solo mostramos los verificados
                }

                // Obtenemos los posts filtrados según el parámetro 'verified' (o todos los verificados por defecto)
                var posts = await _posts.Find(query).ToListAsync();
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
                // Verificar si el post existe
                var existingPost = await _posts.Find(p => p.Id == id).FirstOrDefaultAsync();
                if (existingPost == null)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                // Crear la definición de actualización dinámica
                var updateDefinition = Builders<Post>.Update
                    .Set(p => p.Modified, true); // Aseguramos que 'Modified' siempre se actualice

                // Solo actualizamos 'titulo' si se proporciona en la solicitud
                if (!string.IsNullOrEmpty(updatePost.Titulo) && updatePost.Titulo != existingPost.Titulo)
                {
                    updateDefinition = updateDefinition.Set(p => p.Titulo, updatePost.Titulo);
                }

                // Solo actualizamos 'contenido' si se proporciona en la solicitud
                if (!string.IsNullOrEmpty(updatePost.Contenido) && updatePost.Contenido != existingPost.Contenido)
                {
                    updateDefinition = updateDefinition.Set(p => p.Contenido, updatePost.Contenido);
                }

                // Solo actualizamos 'mensaje_admin' si se proporciona en la solicitud
                if (updatePost.MensajeAdmin != existingPost.MensajeAdmin)
                {
                    updateDefinition = updateDefinition.Set(p => p.MensajeAdmin, updatePost.MensajeAdmin);
                }

                // Solo actualizamos 'verified' si se proporciona en la solicitud
                if (updatePost.Verified != existingPost.Verified)
                {
                    updateDefinition = updateDefinition.Set(p => p.Verified, updatePost.Verified);
                }

                // Solo actualizamos 'oculto' si se proporciona en la solicitud
                if (updatePost.Oculto != existingPost.Oculto)
                {
                    updateDefinition = updateDefinition.Set(p => p.Oculto, updatePost.Oculto);
                }

                // Si no se han proporcionado cambios, no realizamos ninguna actualización
                if (updateDefinition == null)
                {
                    return Ok(new { success = true, message = "No se realizaron cambios." });
                }

                // Realizar la actualización solo con los campos modificados
                var result = await _posts.UpdateOneAsync(p => p.Id == id, updateDefinition);
                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(string id, [FromQuery] string usuarioId)
        {
            try
            {
                var result = await _posts.DeleteOneAsync(p => p.Id == id);
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }
                
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }
}