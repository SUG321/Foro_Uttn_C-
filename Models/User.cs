using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FORO_UTTN_API.Models
{
    [BsonIgnoreExtraElements]
    public class Perfil
    {
        [BsonElement("biografia")]
        public string Biografia { get; set; } = string.Empty;

        [BsonElement("foto_perfil")]
        public string FotoPerfil { get; set; } = string.Empty;
    }
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("apodo")]
        public string Apodo { get; set; } = "Usuario Nuevo";

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [BsonElement("admin")]
        public bool Admin { get; set; } = false;

        [BsonElement("fecha_registro")]
        public DateTime? FechaRegistro { get; set; } = DateTime.UtcNow;

        [BsonElement("perfil")]
        public Perfil Perfil { get; set; } = new();
    }
}