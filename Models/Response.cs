using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{

    public class Response
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("usuario_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = string.Empty; // Referencia al Usuario

        [BsonElement("pregunta_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PreguntaId { get; set; } = string.Empty; // Referencia al Post

        [BsonElement("contenido")]
        public string Contenido { get; set; } = string.Empty;

        [BsonElement("fecha_respuesta")]
        public DateTime FechaRespuesta { get; set; } = DateTime.UtcNow;

        [BsonElement("modified")]
        public bool Modified { get; set; } = false;

        [BsonElement("verified")]
        public bool Verified { get; set; } = false;
        // Ignorar el campo __v
        [BsonIgnore]
        public int V { get; set; }  // Si se incluye en MongoDB pero no lo necesitas.
    }
}