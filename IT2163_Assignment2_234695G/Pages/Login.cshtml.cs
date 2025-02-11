using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using IT2163_Assignment2_234695G.Context;
using System.ComponentModel.DataAnnotations;
using IT2163_Assignment2_234695G.Models;
using Microsoft.AspNetCore.Identity;

namespace IT2163_Assignment2_234695G.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager; // For authentication
        private readonly UserManager<IdentityUser> _userManager;     // For user management
        private readonly ApplicationDbContext _context;               // For application-specific data
        private static Dictionary<string, int> FailedLoginAttempts = new();

        public class InputModel
        {
            [Required, EmailAddress] public string Email { get; set; }
            [Required] public string Password { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Check for too many failed login attempts
            if (FailedLoginAttempts.ContainsKey(Input.Email) && FailedLoginAttempts[Input.Email] >= 3)
            {
                ModelState.AddModelError("LoginError", "Too many failed attempts. Try again later.");
                return Page();
            }

            // Authenticate the user using SignInManager
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, false, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                // Increment failed login attempts
                if (!FailedLoginAttempts.ContainsKey(Input.Email))
                    FailedLoginAttempts[Input.Email] = 0;
                FailedLoginAttempts[Input.Email]++;

                ModelState.AddModelError("LoginError", "Invalid credentials.");
                return Page();
            }

            // Get the IdentityUser
            var identityUser = await _userManager.FindByEmailAsync(Input.Email);
            if (identityUser == null)
            {
                ModelState.AddModelError("LoginError", "User not found.");
                return Page();
            }

            // Get the ApplicationUser linked to the IdentityUser
            var applicationUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.userID == identityUser.Id);
            if (applicationUser == null)
            {
                ModelState.AddModelError("LoginError", "Application user not found.");
                return Page();
            }

            // Set session for the logged-in user
            HttpContext.Session.SetString("UserId", applicationUser.Id.ToString());

            // Log the login action in the AuditLog table
            var auditLog = new AuditLog
            {
                UserID = applicationUser.Id,
                Action = "Login",
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            // Redirect to the home page
            return RedirectToPage("Home");
        }
    }
}
