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

        // Ver todas las acciones de usuarios o uno en específico
        [HttpGet]
        public async Task<IActionResult> GetActions([FromQuery] string? user_id)
        {
            try
            {
                var filter = string.IsNullOrEmpty(user_id)
                    ? Builders<Actions>.Filter.Empty
                    : Builders<Actions>.Filter.Eq(a => a.UserId, user_id);

                var actions = await _actions.Find(filter).ToListAsync();

                foreach (var action in actions)
                {
                    var user = await _users.Find(u => u.Id == action.UserId).FirstOrDefaultAsync();
                    var date = action.ActionDate ?? DateTime.UtcNow;

                    // Crear objeto base con todas las propiedades necesarias
                    var actionData = new Dictionary<string, object?>
                    {
                        ["action_user"] = user?.Apodo ?? "Desconocido",
                        ["action_user_id"] = user?.Id,
                        ["action"] = action.ActionType,
                        ["details"] = action.Details,
                        ["date"] = DateUtils.DateMX(date),
                        ["hour"] = DateUtils.TimeMX(date)
                    };

                    // Vincular el objetivo según su tipo usando el enum
                    switch (action.ObjectiveType)
                    {
                        case ObjectiveType.Post:
                            var post = await _posts.Find(p => p.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (post != null)
                            {
                                actionData["post_id"] = post.Id;
                                actionData["titulo"] = post.Titulo;
                            }
                            else
                            {
                                actionData = new { actionData, objective = "Este post fue eliminado" };
                            }
                            break;

                        case ObjectiveType.User:
                            var userObjective = await _users.Find(u => u.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (userObjective != null)
                            {
                                actionData["user_id"] = userObjective.Id;
                                actionData["apodo"] = userObjective.Apodo;
                            }
                            else
                            {
                                actionData["objective"] = "Este usuario fue eliminado";
                            }
                            break;

                        case ObjectiveType.Response:
                            var response = await _responses.Find(r => r.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (response != null)
                            {
                                actionData["response_id"] = response.Id;
                                actionData["response_content"] = response.Contenido;
                            }
                            else
                            {
                                actionData["objective"] = "Esta respuesta fue eliminada";
                            }
                            break;

                        case ObjectiveType.Faq:
                            var faq = await _faqs.Find(f => f.Id == action.ObjectiveId).FirstOrDefaultAsync();
                            if (faq != null)
                            {
                                actionData["faq_id"] = faq.Id;
                                actionData["faq_content"] = faq.Titulo;
                            }
                            else
                            {
                                actionData["objective"] = "Esta pregunta fue eliminada";
                            }
                            break;

                        default:
                            // Si no hay tipo de objetivo específico, no agregamos información adicional
                            break;
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
                await RegistrarAccion(action.UserId, action.ActionType, action.Details, action.ObjectiveId, action.ObjectiveType);
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