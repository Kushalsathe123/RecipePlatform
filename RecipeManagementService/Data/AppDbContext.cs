using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RecipePlatform.RecipeManagementService.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Data
{
    public class AppDbContext
    {
        private readonly IMongoDatabase _database;
        private IMongoDatabase @object;

        public AppDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var databaseName = configuration["MongoDB:DatabaseName"];

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public AppDbContext(IMongoDatabase @object)
        {
            this.@object = @object;
        }

        public IMongoCollection<Recipe> Recipes => _database.GetCollection<Recipe>("Recipes");
        public IMongoCollection<Like> Likes => _database.GetCollection<Like>("Likes");
    }
}
