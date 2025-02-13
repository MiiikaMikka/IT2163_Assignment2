using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IT2163_Assignment2_234695G.Models
{
    public class UserSession
    {
        [Key]
        public int Id { get; set; } // Primary Key
        public string UserId { get; set; } // Foreign Key to IdentityUser
        public string SessionToken { get; set; } // Unique session identifier
        public DateTime CreatedAt { get; set; } // Timestamp of session creation
        public bool IsActive { get; set; } // Whether the session is active or not

        // Navigation Property to IdentityUser (if needed)
        public IdentityUser User { get; set; }
    }


}
