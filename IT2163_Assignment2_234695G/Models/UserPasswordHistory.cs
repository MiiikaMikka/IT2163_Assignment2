using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IT2163_Assignment2_234695G.Models
{
    public class UserPasswordHistory
    {
        [Key]
        public int Id { get; set; } // Primary Key
        public string UserId { get; set; } // Foreign Key to IdentityUser
        public string PasswordHash { get; set; } // Hash of the old password
        public DateTime DateChanged { get; set; } // Timestamp of when the password was changed

        // Navigation Property to IdentityUser (if needed)
        public IdentityUser User { get; set; }
    }

}
