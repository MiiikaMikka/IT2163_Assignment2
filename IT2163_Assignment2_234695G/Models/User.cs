using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IT2163_Assignment2_234695G.Models
{
    public class ApplicationUser 
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(User))] // Specifies the navigation property name
        public string userID {  get; set; }

        // Navigation property to IdentityUser
        public IdentityUser User { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string EncryptedNRIC { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ResumePath { get; set; }
        public string WhoAmI { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
