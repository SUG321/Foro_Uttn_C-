using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FORO_UTTN_API.Models;
using FORO_UTTN_API.Utils;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ActionModel = FORO_UTTN_API.Models.Action;

namespace FORO_UTTN_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionsController : ControllerBase
    {
        private readonly IMongoCollection<ActionModel> _actions;
        private readonly MongoService _mongoService;
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Post> _posts;
        private readonly IMongoCollection<Response> _responses;
        private readonly IMongoCollection<FAQ> _faqs;

        public ActionsController(MongoService mongoService)
        {
            _mongoService = mongoService;
            _actions = mongoService.Actions;
            _users = mongoService.Users;
            _posts = mongoService.Posts;
            _responses = mongoService.Responses;
            _faqs = mongoService.FAQ;
        }

        // Ver todas las acciones de usuarios o una en específico
        [HttpGet]
        public async Task<IActionResult> GetActions([FromQuery] string? user_id)
        {
            try
            {
                var filter = string.IsNullOrEmpty(user_id)
                    ? Builders<ActionModel>.Filter.Empty
                    : Builders<ActionModel>.Filter.Eq(a => a.UserId, user_id);
                var actions = await _actions.Find(filter).ToListAsync();

                // Formatear las acciones
                var formattedActions = new List<Dictionary<string, object>>();
                foreach (var action in actions)
                {
                    var user = await _users.Find(u => u.Id == action.UserId).FirstOrDefaultAsync();
                    var date = action.ActionDate ?? DateTime.UtcNow;

                    var actionData = new Dictionary<string, object>
                    {
                        ["action_user"] = user?.Apodo ?? "Usuario eliminado",
                        ["action_user_id"] = user?.Id.ToString() ?? "Usuario eliminado",
                        ["action"] = action.ActionType,
                        ["details"] = action.Details ?? string.Empty,
                        ["date"] = DateUtils.DateMX(date),
                        ["hour"] = DateUtils.TimeMX(date)
                    };

                    if (action.ObjectiveType.HasValue)
                    {
                        // Buscar el objetivo de la acción según el tipo
                        if (action.ObjectiveType == ObjectiveType.Post)
                        {
                            var post = await _posts.Find(p => p.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (post != null)
                            {
                                actionData["post_id"] = post.Id.ToString();
                                actionData["titulo"] = post.Titulo;
                            }
                            else
                            {
                                actionData["objective"] = "Este post fue eliminado";
                            }
                        }
                        if (action.ObjectiveType == ObjectiveType.User)
                        {
                            var userObjective = await _users.Find(u => u.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (userObjective != null)
                            {
                                actionData["user_id"] = userObjective.Id.ToString();
                                actionData["apodo"] = userObjective.Apodo;
                            }
                            else
                            {
                                actionData["objective"] = "Este usuario fue eliminado";
                            }
                        }
                        if (action.ObjectiveType == ObjectiveType.Faq)
                        {
                            var response = await _responses.Find(r => r.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (response != null)
                            {
                                actionData["response_id"] = response.Id.ToString();
                                actionData["response_content"] = response.Contenido;
                            }
                            else
                            {
                                actionData["objective"] = "Esta respuesta fue eliminada";
                            }
                        }
                        if (action.ObjectiveType == ObjectiveType.Faq)
                        {
                            var faq = await _faqs.Find(f => f.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (faq != null)
                            {
                                actionData["faq_id"] = faq.Id.ToString();
                                actionData["faq_content"] = faq.Titulo;
                            }
                            else
                            {
                                actionData["objective"] = "Esta pregunta fue eliminada";
                            }
                        }
                    }

                    formattedActions.Add(actionData);
                }

                return Ok(formattedActions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAction([FromBody] ActionModel newAction)
        {
            try
            {
                // Llamamos a RegistrarAccion pasando los parámetros adecuados
                await ActionLogger.RegistrarAccion(
                    _mongoService,
                    newAction.UserId,
                    newAction.ActionType,
                    newAction.Details ?? string.Empty,
                    newAction.ObjectiveId ?? string.Empty,
                    newAction.ObjectiveType?.ToString() ?? string.Empty
                );

                // Retornamos una respuesta exitosa
                return StatusCode(201, new { success = true });
            }
            catch (Exception ex)
            {
                // Si ocurre algún error, regresamos un código 500 con el mensaje de error
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}