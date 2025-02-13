using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using IT2163_Assignment2_234695G.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IT2163_Assignment2_234695G.Models;
using IT2163_Assignment2_234695G.Context;


namespace IT2163_Assignment2_234695G.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ResetPasswordModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public ResetPasswordInputModel Input { get; set; }

        public class ResetPasswordInputModel
        {
            public string Email { get; set; }
            public string Token { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string token, string email)
        {
            Input = new ResetPasswordInputModel { Token = token, Email = email };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Find the user by email
                var user = await _userManager.FindByEmailAsync(Input.Email); // Ensure you use the correct way to retrieve user by email
                if (user == null)
                {
                    Console.WriteLine("User not found");
                    return RedirectToPage("/Login"); // Redirect if user not found
                }

                // Validate against password history
                var passwordHistory = _context.UserPasswordHistory
                                               .Where(ph => ph.UserId == user.Id)
                                               .OrderByDescending(ph => ph.DateChanged)
                                               .Take(2)
                                               .Select(ph => ph.PasswordHash)
                                               .ToList();

                foreach (var pastPasswordHash in passwordHistory)
                {
                    if (_userManager.PasswordHasher.VerifyHashedPassword(user, pastPasswordHash, Input.NewPassword) == PasswordVerificationResult.Success)
                    {
                        ModelState.AddModelError(string.Empty, "You cannot reuse a password used in the last two resets.");
                        return Page();
                    }
                }

                // Reset the password
                var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.NewPassword);
                if (result.Succeeded)
                {
                    // Save the new password hash to history
                    _context.UserPasswordHistory.Add(new UserPasswordHistory
                    {
                        UserId = user.Id,
                        PasswordHash = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword),
                        DateChanged = DateTime.UtcNow
                    });
                    await _context.SaveChangesAsync();

                    return RedirectToPage("/Login"); // Redirect to login after success
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return Page(); // Stay on the page if errors occurred
            }
            return Page();
        }

    }


}
