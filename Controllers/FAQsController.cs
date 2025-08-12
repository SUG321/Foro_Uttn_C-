using System;
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
    public class FAQsController : ControllerBase
    {
        private readonly IMongoCollection<FAQ> _faqs;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<Response> _responses;
        private readonly IMongoCollection<User> _users;
        private readonly MongoService _mongoService;

        public FAQsController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _faqs = mongoService.FAQ;
            _posts = mongoService.Posts;
            _responses = mongoService.Responses;
            _users = mongoService.Users;
        }

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

        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetFaqFromPost(string postId, [FromQuery] string usuarioId)
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
                    UsuarioId = usuarioId,
                    Titulo = post.Titulo,
                    Contenido = verifiedResponse.Contenido
                };

                await _faqs.InsertOneAsync(newFaq);

                await ActionLogger.RegistrarAccion(_mongoService, usuarioId.ToString(), 17, "Agregó una publicación a FAQ", newFaq.Id, "Faq");

                return Ok(new { titulo = post.Titulo, contenido = verifiedResponse.Contenido });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

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

                await ActionLogger.RegistrarAccion(_mongoService, newFaq.UsuarioId, 18, "Agregó una pregunta a FAQ", newFaq.Id, "Faq");

                return StatusCode(201, new { success = true, faq_id = newFaq.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFaq(string id, [FromBody] FAQ updateFaq)
        {
            try
            {
                updateFaq.Id = id;
                var result = await _faqs.ReplaceOneAsync(f => f.Id == id, updateFaq);
                if (result.MatchedCount == 0)
                {
                    return NotFound(new { success = false, message = "FAQ no encontrada" });
                }

                await ActionLogger.RegistrarAccion(_mongoService, updateFaq.UsuarioId, 20, "Modificó una pregunta de FAQ", updateFaq.Id, "Faq");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFaq(string id, [FromQuery] string usuarioId)
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


    }
}