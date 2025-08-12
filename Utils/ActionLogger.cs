using System;
using System.Threading.Tasks;
using FORO_UTTN_API.Models;
using MongoDB.Driver;
using ActionModel = FORO_UTTN_API.Models.Action;

namespace FORO_UTTN_API.Utils
{
    public static class ActionLogger
    {
        public static async Task RegistrarAccion(MongoService mongoService, string userId, int actionType, string details, string objectiveId, string objectiveType)
        {
            if (!Enum.TryParse<ObjectiveType>(objectiveType, true, out var parsedObjectiveType))
            {
                throw new ArgumentException($"El valor '{objectiveType}' no es válido para el tipo ObjectiveType.");
            }

            var action = new ActionModel
            {
                UserId = userId,
                ActionType = actionType,
                Details = details,
                ActionDate = DateTime.UtcNow,
                ObjectiveId = objectiveId,
                ObjectiveType = parsedObjectiveType
            };

            await mongoService.Actions.InsertOneAsync(action);
        }
    }
}