using IT2163_Assignment2_234695G.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IT2163_Assignment2_234695G.Context
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        // DbSets for UserSessions and UserPasswordHistory
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserPasswordHistory> UserPasswordHistory { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the relationship explicitly if needed
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(au => au.User)
                .WithMany()
                .HasForeignKey(au => au.userID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete if necessary

            // Configure the relationship between AuditLog and ApplicationUser
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascading delete


            // Optional: You can configure indexes, relationships, etc. here
            modelBuilder.Entity<UserSession>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted

            modelBuilder.Entity<UserPasswordHistory>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if user is deleted
        }
    }


}



