using IT2163_Assignment2_234695G.Context;
using IT2163_Assignment2_234695G.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace IT2163_Assignment2_234695G.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public ApplicationUser CurrentUser { get; set; }

        public IndexModel(UserManager<IdentityUser> userManager, 
            ApplicationDbContext context, 
            ILogger<IndexModel> logger,
            IDataProtectionProvider dataProtectionProvider)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _dataProtectionProvider = dataProtectionProvider;
        }

        public async Task OnGetAsync()
        {
            // Get the currently logged-in user
            var identityUser = await _userManager.GetUserAsync(User);

            if (identityUser != null)
            {
                // Retrieve the application-specific user details
                CurrentUser = await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.userID == identityUser.Id);

                if (CurrentUser != null && !string.IsNullOrEmpty(CurrentUser.EncryptedNRIC))
                {
                    try
                    {
                        // Decrypt the NRIC
                        var protector = _dataProtectionProvider.CreateProtector("SensitiveDataProtector");
                        CurrentUser.EncryptedNRIC = protector.Unprotect(CurrentUser.EncryptedNRIC);
                    }
                    catch (Exception)
                    {
                        CurrentUser.EncryptedNRIC = "Unable to decrypt NRIC";
                    }
                }

            }
        }
    }
}
