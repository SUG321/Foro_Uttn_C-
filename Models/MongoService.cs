using MongoDB.Driver;

namespace FORO_UTTN_API.Models
{
    public class MongoService
    {
        private readonly IMongoDatabase _database;

        public MongoService()
        {
            var connectionString = "mongodb://localhost:27017";  
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase("foro_uttn");  // Nombre de tu base de datos
        }

        // Colección de usuarios
        public IMongoCollection<Users> Users => _database.GetCollection<Users>("users");

        // Colección de posts
        public IMongoCollection<Posts> Posts => _database.GetCollection<Posts>("posts");

        // Colección de registros (signup)
        public IMongoCollection<SignUp> Signups => _database.GetCollection<SignUp>("signup");

        // Colección de respuestas
        public IMongoCollection<Responses> Responses => _database.GetCollection<Responses>("responses");

        // Colección de registros de inicio de sesión
        public IMongoCollection<Login> Logins => _database.GetCollection<Login>("login");

        //Coleccion de acciones
        public IMongoCollection<Actions> Actions => _database.GetCollection<Actions>("actions");

        //Coleccion de FAQ
        public IMongoCollection<FAQ> FAQ => _database.GetCollection<FAQ>("faqs");


    }
}
