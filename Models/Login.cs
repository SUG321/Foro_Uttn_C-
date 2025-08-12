using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Login
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("contraseña")]
        public string Contraseña { get; set; }

        [BsonElement("fecha_inicio_sesion")]
        public DateTime FechaInicioSesion { get; set; }
        // Ignorar el campo __v
        [BsonIgnore]
        public int V { get; set; }  // Si se incluye en MongoDB pero no lo necesitas.
    }
}
