using FORO_UTTN_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResponsesController : ControllerBase
    {
        private readonly IMongoCollection<Responses> _responses;
        private readonly IMongoCollection<Users> _users;

        public ResponsesController(MongoService mongoService)
        {
            _responses = mongoService.Responses;
            _users = mongoService.Users;
        }

        //GET /posts/:postId/responses  Lista las respuestas de un post.
        [HttpGet("posts/{postId}/responses")]
        public async Task<IActionResult> GetResponsesByPost(string postId)
        {
            try
            {
                var objectId = new ObjectId(postId);
                var responses = await _responses.Find(r => r.PreguntaId == objectId).ToListAsync();
                var result = new List<object>();

                foreach (var resp in responses)
                {
                    var user = await _users.Find(u => u.Id == resp.UsuarioId).FirstOrDefaultAsync();

                    result.Add(new
                    {
                        respuesta_id = resp.Id,
                        contenido = resp.Contenido,
                        fecha_respuesta = resp.FechaRespuesta,
                        votos = resp.Votos?.Count ?? 0,
                        usuario = new
                        {
                            id = user?.Id,
                            apodo = user?.Apodo ?? "Desconocido",
                            nombre = user?.Nombre,
                            foto_perfil = user?.Perfil?.FotoPerfil
                        }
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        //POST /posts/:postId/responses Crea una respuesta.
        [HttpPost("posts/{postId}/responses")]
        public async Task<IActionResult> CreateResponse(string postId, [FromBody] Responses nuevaRespuesta)
        {
            try
            {
                if (string.IsNullOrEmpty(nuevaRespuesta.UsuarioId) || string.IsNullOrEmpty(nuevaRespuesta.Contenido))
                {
                    return BadRequest(new { success = false, message = "usuario_id y contenido son obligatorios." });
                }

                // Validar y convertir postId a ObjectId
                if (!ObjectId.TryParse(postId, out ObjectId preguntaObjectId))
                {
                    return BadRequest(new { success = false, message = "postId inválido." });
                }

                nuevaRespuesta.PreguntaId = preguntaObjectId;
                nuevaRespuesta.FechaRespuesta = DateTime.UtcNow;
                nuevaRespuesta.Modified = false;
                nuevaRespuesta.Votos = nuevaRespuesta.Votos ?? new List<ObjectId>();

                await _responses.InsertOneAsync(nuevaRespuesta);

                return Ok(new { success = true, message = "Respuesta creada correctamente", data = nuevaRespuesta });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("responses/{id}")]
        public async Task<IActionResult> UpdateResponse(string id, [FromBody] Responses updateData)
        {
            try
            {
                var update = Builders<Responses>.Update
                    .Set(r => r.Contenido, updateData.Contenido)
                    .Set(r => r.Modified, true);

                var result = await _responses.UpdateOneAsync(r => r.Id == id, update);

                if (result.MatchedCount == 0)
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });

                return Ok(new { success = true, message = "Respuesta actualizada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("responses/{id}")]
        public async Task<IActionResult> DeleteResponse(string id)
        {
            try
            {
                var result = await _responses.DeleteOneAsync(r => r.Id == id);

                if (result.DeletedCount == 0)
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });

                return Ok(new { success = true, message = "Respuesta eliminada" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
