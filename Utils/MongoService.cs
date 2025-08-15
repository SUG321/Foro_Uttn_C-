using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using FORO_UTTN_API.Models;
using ActionModel = FORO_UTTN_API.Models.Action;

namespace FORO_UTTN_API.Utils
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService(IConfiguration configuration)
        {
            // Obtiene la cadena de conexión desde el archivo appsettings.json
            var connection = configuration.GetConnectionString("MongoDB") ??
                             "mongodb://172.16.0.164:27017"; // Valor por defecto en caso de no encontrar la configuración

            var dbName = configuration.GetValue<string>("DatabaseName") ?? "foro_uttn"; // Valor por defecto

            var client = new MongoClient(connection);  // Crea el cliente de MongoDB con la cadena de conexión
            _database = client.GetDatabase(dbName);   // Se conecta a la base de datos
        }

        public IMongoCollection<FAQ> FAQ => _database.GetCollection<FAQ>("faqs");
        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("posts");
        public IMongoCollection<Response> Responses => _database.GetCollection<Response>("responses");
        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<ActionModel> Actions => _database.GetCollection<ActionModel>("actions");
    }
}
