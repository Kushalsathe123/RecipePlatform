using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecipePlatform.RecipeManagementService.Data.Entities
{
    public class Like
    {
        [BsonId]
        public ObjectId Id { get; set; }
        //[BsonElement("RecipeId")]
        public int RecipeId { get; set; }  // Reference to Recipe
        public int UserId { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
