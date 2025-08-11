using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Responses
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("usuario_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty;

        [BsonElement("pregunta_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PreguntaId { get; set; } = string.Empty; // Referencia al Post (Pregunta)

        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty; // Contenido de la respuesta

        [BsonElement("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow; // Fecha de la respuesta

        [BsonElement("votos")]
        public List<ObjectId> Votos { get; set; } = new List<ObjectId>(); // Votos como array de ObjectIds

        [BsonElement("modified")]
        public bool Modified { get; set; } = false; // Marca si fue modificada

        [BsonElement("verified")]
        public bool Verified { get; set; } = false; // Marca si la respuesta fue verificada
    }
}

