using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty; // Id único para el usuario

        [BsonElement("apodo")]
        public string Apodo { get; set; } = "Usuario Nuevo"; // Apodo, valor por defecto

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty; // Email único y requerido

        [BsonElement("contraseña")]
        public string Contraseña { get; set; } = string.Empty; // Contraseña (debería ser encriptada)

        [BsonElement("admin")]
        public bool Admin { get; set; } = false; // Indica si es un administrador

        [BsonElement("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow; // Fecha de registro por defecto a la fecha actual

        [BsonElement("preguntas_publicadas")]
        public List<ObjectId> PreguntasPublicadas { get; set; } = new List<ObjectId>(); // Lista de Ids de preguntas publicadas por el usuario

        [BsonElement("respuestas_dadas")]
        public List<ObjectId> RespuestasDadas { get; set; } = new List<ObjectId>(); // Lista de Ids de respuestas dadas por el usuario
    }
}