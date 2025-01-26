using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace RecipePlatform.RecipeManagementService.Data.Entities
{
    public class Recipe
    {
        [BsonId]
        public ObjectId Id { get; set; }  // MongoDB ObjectId, but we don't directly use it in the API

        // Map RecipeId to int, and use it in your application
        [BsonElement("RecipeId")]
        public int RecipeId { get; set; }
        public int UserId { get; set; }
        public string RecipeName { get; set; }
        public string Description { get; set; }
        public List<string>? ImageUrls { get; set; } = new List<string>();
        public List<string> Ingredients { get; set; } = new List<string>();
        public string Instructions { get; set; }
        public string CookingTime { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "pending";

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime? ApprovedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdatedAt { get; set; }

        public int LikesCount { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public string Feedback { get; set; }
    }

    public class Comment
    {
        public string UserName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<Comment> Replies { get; set; } = new List<Comment>();
    }

}
