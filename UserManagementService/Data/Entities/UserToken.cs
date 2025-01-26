using System;

namespace RecipePlatform.UserManagementService.Data.Entities
{
    public class UserToken
    {
        public int UserTokenId { get; set; }  // Primary Key for the token
        public int UserId { get; set; }       // Foreign Key to User
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpirationDateTime { get; set; }
        public bool IsInvalid { get; set; } = false; // Indicates if the token is invalidated
        public string TokenType { get; set; } = "jwt-access-token"; // Default to "jwt", can be "forgotpassword" or others

        // Navigation property (optional, if you want EF to track the relationship)
        public User? User { get; set; }
    }


}