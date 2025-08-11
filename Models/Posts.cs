using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Posts
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("usuario_id")]
        public string UsuarioId { get; set; } = string.Empty;

        [BsonElement("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty;

        [BsonElement("fecha_publicacion")]
        public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;

        [BsonElement("respuestas")]
        public List<ObjectId> Respuestas { get; set; } = new List<ObjectId>();

        [BsonElement("votos")]
        public int Votos { get; set; } = 0;  // Este es el valor entero de votos

        [BsonElement("modified")]
        public bool Modified { get; set; } = false;

        [BsonElement("verified")]
        public bool Verified { get; set; } = false;

        [BsonElement("mensaje_admin")]
        public string MensajeAdmin { get; set; } = string.Empty;
    }
}
