using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class SignUp
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("nombre_completo")]
        public string NombreCompleto { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("contraseña")]
        public string Contraseña { get; set; }

        [BsonElement("confirmacion_contraseña")]
        public string ConfirmacionContraseña { get; set; }
    }
}

