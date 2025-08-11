using FORO_UTTN_API.Models;
using FORO_UTTN_API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FAQsController : ControllerBase
    {
        private readonly IMongoCollection<FAQ> _faqs;
        private readonly IMongoCollection<Posts> _posts;
        private readonly IMongoCollection<Responses> _responses;
        private readonly IMongoCollection<Users> _users;
        private readonly MongoService _mongoService;

        public FAQsController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _faqs = mongoService.FAQ;
            _posts = mongoService.Posts;
            _responses = mongoService.Responses;
            _users = mongoService.Users;
        }

        // Obtener todas las FAQs
        [HttpGet]
        public async Task<IActionResult> GetAllFaqs()
        {
            try
            {
                var faqs = await _faqs.Find(_ => true).ToListAsync();
                var formFaqs = faqs.Select(faq => new
                {
                    faq_id = faq.Id,
                    user_id = faq.UsuarioId,
                    titulo = faq.Titulo,
                    contenido = faq.Contenido,
                    pub_date = DateUtils.DateMX(faq.FechaCreacion),
                    pub_time = DateUtils.TimeMX(faq.FechaCreacion)
                }).ToList();

                return Ok(formFaqs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Obtener una FAQ por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFaqById(string id)
        {
            try
            {
                var faq = await _faqs.Find(f => f.Id == id).FirstOrDefaultAsync();
                if (faq == null)
                {
                    return NotFound(new { success = false, message = "FAQ no encontrada" });
                }

                var formFaq = new
                {
                    faq_id = faq.Id,
                    user_id = faq.UsuarioId,
                    titulo = faq.Titulo,
                    contenido = faq.Contenido,
                    pub_date = DateUtils.DateMX(faq.FechaCreacion),
                    pub_time = DateUtils.TimeMX(faq.FechaCreacion)
                };

                return Ok(formFaq);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Obtener FAQ a partir de un post con respuesta verificada
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetFaqFromPost(string postId, [FromBody] dynamic body)
        {
            try
            {
                var post = await _posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
                if (post == null)
                {
                    return NotFound(new { success = false, message = "Post no encontrado" });
                }

                var verifiedResponse = await _responses.Find(r => r.PreguntaId == postId && r.Verified).FirstOrDefaultAsync();
                if (verifiedResponse == null)
                {
                    return NotFound(new { success = false, message = "No existe una respuesta verificada" });
                }

                var newFaq = new FAQ
                {
                    UsuarioId = body.usuario_id,
                    Titulo = post.Titulo,
                    Contenido = verifiedResponse.Contenido
                };

                await _faqs.InsertOneAsync(newFaq);

                // Registrar acción de creación de FAQ
                await RegistrarAccion(body.usuario_id, 17, "Agregó una publicación a FAQ", newFaq.Id, "Faq");

                return Ok(new { titulo = post.Titulo, contenido = verifiedResponse.Contenido });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Crear una nueva FAQ
        [HttpPost]
        public async Task<IActionResult> CreateFaq([FromBody] FAQ newFaq)
        {
            try
            {
                if (string.IsNullOrEmpty(newFaq.Titulo) || string.IsNullOrEmpty(newFaq.Contenido))
                {
                    return BadRequest(new { success = false, message = "Campos requeridos están vacíos." });
                }

                await _faqs.InsertOneAsync(newFaq);

                // Registrar acción de crear FAQ
                await RegistrarAccion(newFaq.UsuarioId, 18, "Agregó una pregunta a FAQ", newFaq.Id, "Faq");

                return StatusCode(201, new { success = true, faq_id = newFaq.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Actualizar una FAQ existente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFaq(string id, [FromBody] FAQ updateFaq)
        {
            try
            {
                var result = await _faqs.ReplaceOneAsync(f => f.Id == id, updateFaq);
                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "FAQ no encontrada" });
                }

                // Registrar acción de modificar FAQ
                await RegistrarAccion(updateFaq.UsuarioId, 20, "Modificó una pregunta de FAQ", updateFaq.Id, "Faq");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Eliminar una FAQ
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(string id, [FromBody] dynamic body)
        {
            try
            {
                var result = await _faqs.DeleteOneAsync(f => f.Id == id);
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "FAQ no encontrada" });
                }

                // Registrar acción de eliminar FAQ
                await RegistrarAccion(body.usuario_id, 19, "Eliminó una pregunta de FAQ", id, "Faq");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Update the RegistrarAccion method to correctly assign the ObjectiveType enum value
        private async Task RegistrarAccion(string userId, int actionType, string details, string objectiveId, string objectiveType)
        {
            // Parse the string objectiveType to the ObjectiveType enum
            if (!Enum.TryParse<ObjectiveType>(objectiveType, true, out var parsedObjectiveType))
            {
                throw new ArgumentException($"El valor '{objectiveType}' no es válido para el tipo ObjectiveType.");
            }

            var action = new Actions
            {
                UserId = userId,
                ActionType = actionType,
                Details = details,
                ActionDate = DateTime.UtcNow,
                ObjectiveId = objectiveId,
                ObjectiveType = parsedObjectiveType // Correctly assign the parsed enum value
            };

            await _mongoService.Actions.InsertOneAsync(action);
        }
    }
}
