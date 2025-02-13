using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using IT2163_Assignment2_234695G.Context;
using System.ComponentModel.DataAnnotations;
using IT2163_Assignment2_234695G.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity.UI.Services;
using IT2163_Assignment2_234695G.Utils;
using System;

namespace IT2163_Assignment2_234695G.Pages
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        private readonly IConfiguration _configuration; 

        private static Dictionary<string, int> FailedLoginAttempts = new();



        public LoginModel(SignInManager<IdentityUser> signInManager, 
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, 
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;

        }

        public class InputModel()
        {

            [Required, EmailAddress] public string Email { get; set; }
            [Required] public string Password { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public void OnGet()
        {
        }

        // Method to verify reCAPTCHA response using Google API

        private async Task<bool> VerifyCaptchaAsync(string recaptchaResponse)
        {
            var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
            var client = new HttpClient();
            var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(new[] {
        new KeyValuePair<string, string>("secret", secretKey),
        new KeyValuePair<string, string>("response", recaptchaResponse),
    }));

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(jsonResponse);

            return json.Value<bool>("success");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Get the reCAPTCHA response from the form
            var recaptchaResponse = Request.Form["recaptchaResponse"];

            // Verify the reCAPTCHA response
            var isCaptchaValid = await VerifyCaptchaAsync(recaptchaResponse);

            if (!isCaptchaValid)
            {
                ModelState.AddModelError("LoginError", "reCAPTCHA validation failed. Please try again.");
                return Page();
            }

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

            // Continue with the session setup, logging, and redirect
            // Get the IdentityUser
            var identityUser = await _userManager.FindByEmailAsync(Input.Email);
            if (identityUser == null)
            {
                ModelState.AddModelError("LoginError", "User not found.");
                return Page();
            }

            // Setting/Invalidating Session
            var sessionToken = Guid.NewGuid().ToString();
            var existingSessions = _context.UserSessions.Where(s => s.UserId == identityUser.Id && s.IsActive);
            foreach (var session in existingSessions)
            {
                session.IsActive = false;
            }

            // Add new session
            var newSession = new UserSession
            {
                UserId = identityUser.Id,
                SessionToken = sessionToken,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.UserSessions.Add(newSession);
            await _context.SaveChangesAsync();

            // Find the ApplicationUser using IdentityUser's email or Id
            var applicationUser = await _context.ApplicationUsers
                .FirstOrDefaultAsync(u => u.userID == identityUser.Id); // Use correct field for lookup


            // Log the login action
            var auditLog = new AuditLog
            {
                UserID = applicationUser.Id,
                Action = "Login",
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
                
            // User exists and password is correct, generate OTP
            var otp = new Random().Next(100000, 999999).ToString(); // Generate a 6-digit OTP
            var otpSubject = "Your One-Time Password (OTP)";
            var otpMessage = $"Your OTP for login is: {otp}";

            // Store OTP in TempData (valid for the next request)
            TempData["OTPCode"] = otp;
            TempData["OTPExpiry"] = DateTime.UtcNow.AddMinutes(5); // Set expiry time


            // Send OTP via email
            await _emailSender.SendEmailAsync(identityUser.Email, otpSubject, otpMessage);

            // Store the OTP in TempData or any storage for the user
            TempData["OTP"] = otp;

            // Prompt for OTP
            return RedirectToPage("/OTPVerify");
        }
    }
}
