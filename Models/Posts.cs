using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Posts
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("usuario_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; }

        [BsonElement("apodo")]
        public string Apodo { get; set; }

        [BsonElement("titulo")]
        public string Titulo { get; set; }

        [BsonElement("contenido")]
        public string Contenido { get; set; }

        [BsonElement("categoria_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoriaId { get; set; }

        [BsonElement("fecha_publicacion")]
        public DateTime FechaPublicacion { get; set; } = DateTime.Now;

        [BsonElement("respuestas")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Respuestas { get; set; } = new();

        [BsonElement("votos")]
        public int Votos { get; set; }

        [BsonElement("modified")]
        public bool Modified { get; set; } = false;
    }
}
