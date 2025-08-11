using FORO_UTTN_API.Models;
using FORO_UTTN_API.Utils;
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
        private readonly IMongoCollection<Posts> _posts;
        private readonly MongoService _mongoService;

        public ResponsesController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _responses = mongoService.Responses;
            _users = mongoService.Users;
            _posts = mongoService.Posts;
        }

        // Obtener todas las respuestas de un post
        [HttpGet("posts/{postId}/responses")]
        public async Task<IActionResult> GetResponsesByPost(string postId)
        {
            try
            {
                if (!ObjectId.TryParse(postId, out _))
                {
                    return BadRequest(new { success = false, message = "ID de post inválido" });
                }

                var responses = await _responses.Find(r => r.PreguntaId == postId).ToListAsync();
                var responseDetails = new List<object>();

                foreach (var resp in responses)
                {
                    var user = await _users.Find(u => u.Id == resp.UsuarioId).FirstOrDefaultAsync();

                    var responseData = new
                    {
                        pregunta_id = postId,
                        respuesta_id = resp.Id,
                        contenido = resp.Contenido,
                        res_date = DateUtils.DateMX(resp.FechaRespuesta),
                        res_time = DateUtils.TimeMX(resp.FechaRespuesta),
                        votos = resp.Votos.Count, // El número de votos
                        modified = resp.Modified,
                        verified = resp.Verified,
                        usuario = new
                        {
                            id = user?.Id,
                            apodo = user?.Apodo ?? "Desconocido"
                        }
                    };

                    responseDetails.Add(responseData);
                }

                return Ok(responseDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Crear una respuesta para un post
        [HttpPost("posts/{postId}/responses")]
        public async Task<IActionResult> CreateResponse(string postId, [FromBody] Responses newResponse)
        {
            try
            {
                // Validaciones
                if (!ObjectId.TryParse(postId, out _))
                {
                    return BadRequest(new { success = false, message = "ID de post inválido" });
                }

                if (!ObjectId.TryParse(newResponse.UsuarioId, out _))
                {
                    return BadRequest(new { success = false, message = "ID de usuario inválido" });
                }

                if (string.IsNullOrWhiteSpace(newResponse.Contenido))
                {
                    return BadRequest(new { success = false, message = "El contenido de la respuesta es requerido" });
                }

                // Asignar valores
                newResponse.PreguntaId = postId;
                newResponse.FechaRespuesta = DateTime.UtcNow;
                newResponse.Votos = new List<ObjectId>();
                newResponse.Modified = false;
                newResponse.Verified = false;

                await _responses.InsertOneAsync(newResponse);

                // Registrar acción de creación de respuesta
                await RegistrarAccion(newResponse.UsuarioId, 6, "Respondió a una publicación", newResponse.Id, ObjectiveType.Response);

                return CreatedAtAction(nameof(GetResponsesByPost), new { postId = postId }, new { success = true, respuesta_id = newResponse.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private async Task RegistrarAccion(string userId, int actionType, string details, string objectiveId, string objectiveType)
        {
            try
            {
                // Convertimos el string objectiveType a su correspondiente enum ObjectiveType
                if (Enum.TryParse<ObjectiveType>(objectiveType, out var objectiveEnum))
                {
                    var action = new Actions
                    {
                        UserId = userId,
                        ActionType = actionType,
                        Details = details,
                        ActionDate = DateTime.UtcNow,
                        ObjectiveId = objectiveId,
                        ObjectiveType = objectiveEnum // Usamos el enum
                    };

                    await _mongoService.Actions.InsertOneAsync(action);
                }
                else
                {
                    throw new ArgumentException("Tipo de objetivo no válido");
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                Console.WriteLine($"Error al registrar acción: {ex.Message}");
                throw;
            }
        }


        // Actualizar una respuesta existente
        [HttpPut("responses/{id}")]
        public async Task<IActionResult> UpdateResponse(string id, [FromBody] Responses updateData)
        {
            try
            {
                if (!ObjectId.TryParse(id, out _))
                {
                    return BadRequest(new { success = false, message = "ID de respuesta inválido" });
                }

                if (string.IsNullOrWhiteSpace(updateData.Contenido))
                {
                    return BadRequest(new { success = false, message = "El contenido de la respuesta es requerido" });
                }

                var update = Builders<Responses>.Update
                    .Set(r => r.Contenido, updateData.Contenido)
                    .Set(r => r.Modified, true);

                var result = await _responses.UpdateOneAsync(r => r.Id == id, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });
                }

                await RegistrarAccion(updateData.UsuarioId, 7, "Modificó su respuesta", id, ObjectiveType.Response);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eliminar una respuesta
        [HttpDelete("responses/{id}")]
        public async Task<IActionResult> DeleteResponse(string id, [FromBody] dynamic body)
        {
            try
            {
                if (!ObjectId.TryParse(id, out _))
                {
                    return BadRequest(new { success = false, message = "ID de respuesta inválido" });
                }

                var result = await _responses.DeleteOneAsync(r => r.Id == id);
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });
                }

                await RegistrarAccion((string)body.usuario_id, 8, "Eliminó su respuesta", id, ObjectiveType.Response);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Marcar respuesta como verificada
        [HttpPut("responses/{id}/verify")]
        public async Task<IActionResult> VerifyResponse(string id, [FromBody] dynamic body)
        {
            try
            {
                if (!ObjectId.TryParse(id, out _))
                {
                    return BadRequest(new { success = false, message = "ID de respuesta inválido" });
                }

                var update = Builders<Responses>.Update.Set(r => r.Verified, true);
                var result = await _responses.UpdateOneAsync(r => r.Id == id, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });
                }

                await RegistrarAccion((string)body.usuario_id, 9, "Verificó una respuesta", id, ObjectiveType.Response);

                return Ok(new { success = true, message = "Respuesta verificada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Votar una respuesta
        [HttpPost("responses/{id}/vote")]
        public async Task<IActionResult> VoteResponse(string id, [FromBody] dynamic body)
        {
            try
            {
                if (!ObjectId.TryParse(id, out _))
                {
                    return BadRequest(new { success = false, message = "ID de respuesta inválido" });
                }

                if (!ObjectId.TryParse((string)body.usuario_id, out ObjectId usuarioObjectId))
                {
                    return BadRequest(new { success = false, message = "ID de usuario inválido" });
                }

                var response = await _responses.Find(r => r.Id == id).FirstOrDefaultAsync();
                if (response == null)
                {
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });
                }

                // Verificar si el usuario ya votó
                if (response.Votos.Contains(usuarioObjectId))
                {
                    // Remover voto
                    var removeUpdate = Builders<Responses>.Update.Pull(r => r.Votos, usuarioObjectId);
                    await _responses.UpdateOneAsync(r => r.Id == id, removeUpdate);

                    return Ok(new { success = true, message = "Voto removido", votos = response.Votos.Count - 1 });
                }
                else
                {
                    // Agregar voto
                    var addUpdate = Builders<Responses>.Update.Push(r => r.Votos, usuarioObjectId);
                    await _responses.UpdateOneAsync(r => r.Id == id, addUpdate);

                    await RegistrarAccion((string)body.usuario_id, 10, "Votó una respuesta", id, ObjectiveType.Response);

                    return Ok(new { success = true, message = "Voto agregado", votos = response.Votos.Count + 1 });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private async Task RegistrarAccion(string usuario_id, int v1, string v2, string id, ObjectiveType response)
        {
            throw new NotImplementedException();
        }
    }
}
