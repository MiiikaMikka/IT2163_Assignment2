using System.ComponentModel.DataAnnotations;

namespace IT2163_Assignment2_234695G.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        // Foreign key linking to ApplicationUser
        public int UserID { get; set; }

        // Navigation property (optional)
        public ApplicationUser User { get; set; }

        // Action performed by the user (e.g., "Login", "Logout")
        public string Action { get; set; }

        // Timestamp of the action
        public DateTime Timestamp { get; set; }
    }
}
