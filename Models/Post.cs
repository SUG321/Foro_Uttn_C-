using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Post
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("usuario_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty; // Referencia al Usuario

        [BsonElement("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty;

        [BsonElement("fecha_publicacion")]
        public DateTime? FechaPublicacion { get; set; }

        [BsonElement("respuestas")]
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> Respuestas { get; set; } = new();

        [BsonElement("modified")]
        public bool Modified { get; set; } = false;

        [BsonElement("verified")]
        public bool Verified { get; set; } = false;

        [BsonElement("mensaje_admin")]
        public string? MensajeAdmin { get; set; }
        // Ignorar el campo __v
        [BsonIgnore]
        public int V { get; set; }  // Si se incluye en MongoDB pero no lo necesitas.
    }
}