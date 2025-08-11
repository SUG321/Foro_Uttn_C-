using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class FAQ
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("usuario_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty; // Referencia al Usuario

        [BsonElement("titulo")]
        public string Titulo { get; set; } = string.Empty; // Título de la FAQ

        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty; // Contenido de la FAQ

        [BsonElement("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // Fecha de creación de la FAQ
    }
}
