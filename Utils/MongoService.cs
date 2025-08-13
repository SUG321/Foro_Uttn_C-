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
            var connection = configuration.GetConnectionString("MongoDb") ?? "mongodb://172.16.0.164:27017";
            var dbName = configuration.GetValue<string>("DatabaseName") ?? "foro_uttn";
            var client = new MongoClient(connection);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<FAQ> FAQ => _database.GetCollection<FAQ>("faqs");
        public IMongoCollection<Post> Posts => _database.GetCollection<Post>("posts");
        public IMongoCollection<Response> Responses => _database.GetCollection<Response>("responses");
        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<ActionModel> Actions => _database.GetCollection<ActionModel>("actions");
    }
}