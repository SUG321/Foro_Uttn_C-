using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public enum ObjectiveType
    {
        Post,
        Response,
        User,
        Faq
    }
    [BsonIgnoreExtraElements]
    public class Action
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("user_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = string.Empty; // Referencia al Usuario

        [BsonElement("action_type")]
        public int ActionType { get; set; }

        [BsonElement("details")]
        public string? Details { get; set; }

        [BsonElement("action_date")]
        public DateTime? ActionDate { get; set; }

        [BsonElement("objective_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ObjectiveId { get; set; }

        [BsonElement("objective_type")]
        [BsonRepresentation(BsonType.String)]
        public ObjectiveType? ObjectiveType { get; set; }
    }
}