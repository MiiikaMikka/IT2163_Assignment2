using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace IT2163_Assignment2_234695G.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender; // Add email sender service to send reset links

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public string Email { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return RedirectToPage("/Logout"); // If user not found, redirect to login page
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Page(
                "/ResetPassword",
                pageHandler: null,
                values: new { token = token, email = user.Email },
                protocol: Request.Scheme
                );
            

            // Send reset link via email (You must configure your IEmailSender for actual email sending)
            await _emailSender.SendEmailAsync(Email, "Password Reset Request", $"Click here to reset your password: {resetLink}");

            return RedirectToPage("/Login"); // Redirect user to login page after sending email
        }
    }
}
