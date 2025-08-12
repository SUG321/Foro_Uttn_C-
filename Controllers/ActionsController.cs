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
    public class ActionsController(MongoService mongoService) : ControllerBase
    {
        private readonly IMongoCollection<ActionModel> _actions = mongoService.Actions;
        private readonly MongoService _mongoService = mongoService;
        private readonly IMongoCollection<User> _users = mongoService.Users;
        private readonly IMongoCollection<Post> _posts = mongoService.Posts;
        private readonly IMongoCollection<Response> _responses = mongoService.Responses;
        private readonly IMongoCollection<FAQ> _faqs = mongoService.FAQ;

        // Ver todas las acciones de usuarios o una en específico
        [HttpGet]
        public async Task<IActionResult> GetActions([FromQuery] string? user_id)
        {
            try
            {
                // Aplicar filtro si se proporciona un user_id
                var filter = string.IsNullOrEmpty(user_id) ? Builders<ActionModel>.Filter.Empty : Builders<ActionModel>.Filter.Eq(a => a.UserId, user_id);
                var actions = await _actions.Find(filter).ToListAsync();

                // Formatear las acciones
                var formattedActions = new List<object>();
                foreach (var action in actions)
                {
                    var user = await _users.Find(u => u.Id == action.UserId).FirstOrDefaultAsync();
                    var date = action.ActionDate ?? DateTime.UtcNow;

                    var actionData = new
                    {
                        action_user = user?.Apodo ?? "Usuario eliminado",
                        action_user_id = user?.Id.ToString() ?? "Usuario eliminado",
                        action = action.ActionType,
                        details = action.Details,
                        date = DateUtils.DateMX(date),
                        hour = DateUtils.TimeMX(date)
                    };

                    if (!string.IsNullOrEmpty(action.ObjectiveType))
                    {
                        // Buscar el objetivo de la acción según el tipo
                        if (action.ObjectiveType == "Post")
                        {
                            var post = await _posts.Find(p => p.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (post != null)
                            {
                                actionData = new
                                {
                                    action_user = actionData.action_user,
                                    action_user_id = actionData.action_user_id,
                                    action = actionData.action,
                                    details = actionData.details,
                                    date = actionData.date,
                                    hour = actionData.hour,
                                    post_id = post.Id.ToString(),
                                    titulo = post.Titulo
                                };
                            }
                            else
                            {
                                actionData = new { actionData, objective = "Este post fue eliminado" };
                            }
                        }
                        if (action.ObjectiveType == "User")
                        {
                            var userObjective = await _users.Find(u => u.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (userObjective != null)
                            {
                                actionData = new
                                {
                                    action_user = actionData.action_user,
                                    action_user_id = actionData.action_user_id,
                                    action = actionData.action,
                                    details = actionData.details,
                                    date = actionData.date,
                                    hour = actionData.hour,
                                    user_id = userObjective.Id.ToString(),
                                    apodo = userObjective.Apodo
                                };
                            }
                            else
                            {
                                actionData = new { actionData, objective = "Este usuario fue eliminado" };
                            }
                        }
                        if (action.ObjectiveType == "Response")
                        {
                            var response = await _responses.Find(r => r.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (response != null)
                            {
                                actionData = new
                                {
                                    action_user = actionData.action_user,
                                    action_user_id = actionData.action_user_id,
                                    action = actionData.action,
                                    details = actionData.details,
                                    date = actionData.date,
                                    hour = actionData.hour,
                                    response_id = response.Id.ToString(),
                                    response_content = response.Contenido
                                };
                            }
                            else
                            {
                                actionData = new { actionData, objective = "Esta respuesta fue eliminada" };
                            }
                        }
                        if (action.ObjectiveType == "Faq")
                        {
                            var faq = await _faqs.Find(f => f.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (faq != null)
                            {
                                actionData = new
                                {
                                    action_user = actionData.action_user,
                                    action_user_id = actionData.action_user_id,
                                    action = actionData.action,
                                    details = actionData.details,
                                    date = actionData.date,
                                    hour = actionData.hour,
                                    faq_id = faq.Id.ToString(),
                                    faq_content = faq.Titulo
                                };
                            }
                            else
                            {
                                actionData = new { actionData, objective = "Esta pregunta fue eliminada" };
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
                    newAction.Details,
                    newAction.ObjectiveId,
                    newAction.ObjectiveType.ToString()
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