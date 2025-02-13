using IT2163_Assignment2_234695G.Context;
using IT2163_Assignment2_234695G.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IT2163_Assignment2_234695G.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager; // Use IdentityUser for user management
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(UserManager<IdentityUser> userManager,
                             SignInManager<IdentityUser> signInManager,
                             ApplicationDbContext context,
                             IDataProtectionProvider dataProtectionProvider,
                             ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required] public string FirstName { get; set; }
            [Required] public string LastName { get; set; }
            [Required, EmailAddress] public string Email { get; set; }
            [Required] public string Password { get; set; }
            [Required] public string ConfirmPassword { get; set; }
            [Required] public string Gender { get; set; } // Ensure Gender is included
            [Required] public string NRIC { get; set; } // NRIC field

            public string WhoAmI { get; set; } // Add WhoAmI field
            public IFormFile ResumeFile { get; set; }
        }


        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (ModelState.IsValid)
                {

                    string basePath = @"C:\Users\DonTJ\Desktop\School\Year 2\Year 2 Sem 2\Application Security\Assignment_2\IT2163_Assignment2_234695G\IT2163_Assignment2_234695G\Resumes";

                    var filePath = Path.Combine(basePath, Input.ResumeFile.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Input.ResumeFile.CopyToAsync(stream);
                    }

                    // Validate password complexity
                    if (!Regex.IsMatch(Input.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$"))
                    {
                        ModelState.AddModelError("Input.Password", "Password must be at least 12 characters long and contain uppercase, lowercase, number, and special character.");
                        return Page();
                    }

                    // Check if email is already in use
                    var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Input.Email", "Email already exists.");
                        return Page();
                    }

                    // Create a new IdentityUser
                    var identityUser = new IdentityUser
                    {
                        UserName = Input.Email,
                        Email = Input.Email
                    };

                    // Protect sensitive data
                    var protector = _dataProtectionProvider.CreateProtector("SensitiveDataProtector");

                    string encryptedNRIC = protector.Protect(Input.NRIC); // Encrypt NRIC


                    // Add the IdentityUser to the database
                    var createUserResult = await _userManager.CreateAsync(identityUser, Input.Password);

                    if (createUserResult.Succeeded)
                    {
                        Console.WriteLine("Registering Application user into DB");
                        // Create the ApplicationUser and link it to the IdentityUser
                        var applicationUser = new ApplicationUser
                        {
                            userID = identityUser.Id, // Link to the IdentityUser's Id
                            FirstName = Input.FirstName,
                            LastName = Input.LastName,
                            Gender = Input.Gender, // Set Gender
                            Email = Input.Email,

                            EncryptedNRIC = encryptedNRIC, // Store the encrypted NRIC
                            WhoAmI = Input.WhoAmI, // Set WhoAmI field

                            ResumePath = filePath,

                            PasswordHash = identityUser.PasswordHash, // Storing the hashed password
                            CreatedAt = DateTime.UtcNow,
                        };
                        Console.WriteLine("Saving into DB");
                        // Add the ApplicationUser to the database
                        _context.ApplicationUsers.Add(applicationUser);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Created into DB Successfully");
                        // Sign in the user after registration
                        await _signInManager.SignInAsync(identityUser, isPersistent: false);

                        // Redirect to login or dashboard
                        return RedirectToPage("/Login");
                    }
                    Console.WriteLine(createUserResult.Errors);
                    // Handle errors from user creation
                    foreach (var error in createUserResult.Errors)
                    {
                        Console.WriteLine($"Error Code: {error.Code}, Description: {error.Description}");
                        _logger.LogError($"Error Code: {error.Code}, Description: {error.Description}");

                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return Page();

                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                _logger.LogError(ex, "An unexpected error occurred during registration.");
            }

            return Page();
        }

    }
}
