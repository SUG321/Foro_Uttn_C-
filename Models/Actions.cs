using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Actions
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty; // Referencia al Usuario

        [BsonElement("action_type")]
        public int ActionType { get; set; } // Tipo de acción (como número)

        [BsonElement("details")]
        public string Details { get; set; } = string.Empty; // Descripción de la acción

        [BsonElement("action_date")]
        public DateTime ActionDate { get; set; } = DateTime.UtcNow; // Fecha de la acción

        [BsonElement("objective_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ObjectiveId { get; set; } = string.Empty; // ID del objetivo (relacionado con User, Post, Response, etc.)

        [BsonElement("objective_type")]
        public ObjectiveType ObjectiveType { get; set; } // Tipo de objetivo (User, Post, Response, etc.)

        [BsonElement("__v")]
        public int V { get; set; } // Versión del documento

        // Removed invalid operator to fix CS0555
    }

    // Enum para representar los tipos de objetivos relacionados con la acción
    public enum ObjectiveType
    {
        Post,
        User,
        Response,
        Faq
    }
}



