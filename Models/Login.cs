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
        public string ContraseñaHash { get; set; }

        [BsonElement("fecha_inicio_sesion")]
        public DateTime FechaInicioSesion { get; set; }

        [BsonElement("token")]
        public string Token { get; set; } // Si no usas JWT puedes dejarlo como "N/A"
    }
}
