using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using IT2163_Assignment2_234695G.Context;
using IT2163_Assignment2_234695G.Models;
using Microsoft.EntityFrameworkCore;

namespace IT2163_Assignment2_234695G.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the IdentityUser's ID
            var identityUserId = await _userManager.GetUserAsync(User); // Get logged-in user email/username
            if (identityUserId != null)
            {
                Console.WriteLine(identityUserId);
                // Find the ApplicationUser using IdentityUser's email or Id
                var applicationUser = await _context.ApplicationUsers
                    .FirstOrDefaultAsync(u => u.userID == identityUserId.Id); // Use correct field for lookup

                if (applicationUser != null)
                {
                    // Log the logout action in the AuditLog table
                    var auditLog = new AuditLog
                    {
                        UserID = applicationUser.Id, // Integer ID of the ApplicationUser
                        Action = "Logout",
                        Timestamp = DateTime.UtcNow
                    };

                    _context.AuditLogs.Add(auditLog);
                    await _context.SaveChangesAsync();
                }

                // Sign out the user
                await _signInManager.SignOutAsync();

                // Clear the session
                HttpContext.Session.Clear();

                // Redirect to the login page
                return RedirectToPage("Login");
            }
            else
            {
                // User not logged in
                return RedirectToPage("Login");
            }
 
        }
    }
}
