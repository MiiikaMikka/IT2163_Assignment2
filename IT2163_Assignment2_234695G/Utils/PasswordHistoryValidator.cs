using IT2163_Assignment2_234695G.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IT2163_Assignment2_234695G.Utils
{
    public class PasswordHistoryValidator : IPasswordValidator<IdentityUser>
    {
        private readonly ApplicationDbContext _context;

        public PasswordHistoryValidator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string password)
        {
            // Get the last two passwords from the database
            var lastPasswords = await _context.UserPasswordHistory
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.DateChanged)
                .Take(2)
                .ToListAsync();

            foreach (var lastPassword in lastPasswords)
            {
                if (BCrypt.Net.BCrypt.Verify(password, lastPassword.PasswordHash)) // Compare password hashes
                {
                    return IdentityResult.Failed(new IdentityError { Description = "You cannot reuse a previous password." });
                }
            }

            return IdentityResult.Success;
        }
    }

}
