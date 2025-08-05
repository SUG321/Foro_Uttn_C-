using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    public class Perfil
    {
        [BsonElement("biografia")]
        public string Biografia { get; set; }

        [BsonElement("foto_perfil")]
        public string FotoPerfil { get; set; } // URL
    }

    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("apodo")]
        public string Apodo { get; set; } = "Usuario Nuevo";

        [BsonElement("nombre")]
        public string Nombre { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("contraseña")]
        public string Contrasena { get; set; }

        [BsonElement("admin")]
        public bool Admin { get; set; }

        [BsonElement("fecha_registro")]
        [BsonIgnoreIfNull]
        public DateTime? FechaRegistro { get; set; }

        [BsonElement("perfil")]
        public Perfil Perfil { get; set; }

        [BsonElement("preguntas_publicadas")]
        public List<ObjectId> PreguntasPublicadas { get; set; } = new();

        [BsonElement("respuestas_dadas")]
        public List<ObjectId> RespuestasDadas { get; set; } = new();


        [BsonElement("__v")]
        [BsonIgnoreIfDefault]
        public int Version { get; set; }
    }
}