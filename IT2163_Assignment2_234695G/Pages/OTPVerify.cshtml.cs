using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IT2163_Assignment2_234695G.Pages
{
    public class OTPVerifyModel : PageModel
    {
        private readonly IEmailSender _emailSender;

        public OTPVerifyModel(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [BindProperty]
        public string OTP { get; set; }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            // Get the OTP stored in TempData (or from a database session, etc.)
            var storedOtp = TempData["OTP"]?.ToString();

            if (storedOtp == null)
            {
                ModelState.AddModelError(string.Empty, "OTP is expired or invalid.");
                Console.WriteLine("Stored OTP is null");
                return RedirectToPage("/Login");
            }

            if (OTP == storedOtp)
            {
                // OTP matches, authenticate the user
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Invalid OTP.");
            return Page();
        }
    }
}
