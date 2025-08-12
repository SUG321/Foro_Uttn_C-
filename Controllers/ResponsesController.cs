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
    public class ResponsesController : ControllerBase
    {
        private readonly IMongoCollection<Response> _responses;
        private readonly IMongoCollection<User> _users;
        private readonly MongoService _mongoService;

        public ResponsesController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _responses = mongoService.Responses;
            _users = mongoService.Users;
        }

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetResponsesByPost(string postId)
        {
            try
            {
                var responses = await _responses.Find(r => r.PreguntaId == postId).ToListAsync();
                var formatted = new List<object>();
                foreach (var resp in responses)
                {
                    var user = await _users.Find(u => u.Id == resp.UsuarioId).FirstOrDefaultAsync();
                    var date = resp.FechaRespuesta;
                    formatted.Add(new
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
                    });
                }
                return Ok(formatted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("post/{postId}")]
        public async Task<IActionResult> CreateResponse(string postId, [FromBody] Response newResponse)
        {
            try
            {
                newResponse.PreguntaId = postId;
                newResponse.FechaRespuesta = DateTime.UtcNow;
                await _responses.InsertOneAsync(newResponse);
                await ActionLogger.RegistrarAccion(_mongoService, newResponse.UsuarioId, 6, "Respondió a una publicación", newResponse.Id, "Response");
                return StatusCode(201, new { success = true, respuesta_id = newResponse.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResponse(string id, [FromBody] Response update)
        {
            try
            {
                update.Id = id;
                update.Modified = true;
                var result = await _responses.ReplaceOneAsync(r => r.Id == id, update);
                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });
                }
                await ActionLogger.RegistrarAccion(_mongoService, update.UsuarioId, 7, "Modificó su respuesta", id, "Response");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResponse(string id, [FromQuery] string usuarioId)
        {
            try
            {
                var deleted = await _responses.FindOneAndDeleteAsync(r => r.Id == id);
                if (deleted == null)
                {
                    return NotFound(new { success = false, message = "Respuesta no encontrada" });
                }
                await ActionLogger.RegistrarAccion(_mongoService, usuarioId, 8, "Eliminó su respuesta", deleted.PreguntaId, "Post");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


    }
}