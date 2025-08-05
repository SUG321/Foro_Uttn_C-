using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Responses
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("usuario_id")]
        public string UsuarioId { get; set; }

        [BsonElement("pregunta_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId PreguntaId { get; set; }

        [BsonElement("contenido")]
        public string Contenido { get; set; }

        [BsonElement("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; } = DateTime.Now;

        [BsonElement("votos")]
        public List<ObjectId> Votos { get; set; } = new();

        [BsonElement("modified")]
        public bool Modified { get; set; } = false;
    }
}
