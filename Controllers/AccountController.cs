using Microsoft.AspNetCore.Mvc; // For creating controllers and handling HTTP requests.
using Microsoft.AspNetCore.Identity; // For managing user and role identities.
using System.Linq; // For LINQ operations to query collections.
using System.Threading.Tasks; // For asynchronous programming.
using Sashiel_ST10028058_PROG6212_Part2.Models; // Importing models specific to the project.

namespace Sashiel_ST10028058_PROG6212_Part2.Areas.Identity.Pages.Account
{
    // Controller for handling account-related operations.
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager; // Service to manage user identity.
        private readonly RoleManager<IdentityRole> _roleManager; // Service to manage roles.

        // Constructor initializes UserManager and RoleManager services.
        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Account/Index - Retrieves a list of users and their roles.
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList(); // Retrieves all registered users.
            var userList = new List<UserViewModel>(); // List to hold user details for the view.

            // Loop through each user and fetch their roles.
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user); // Retrieves roles assigned to the user.
                userList.Add(new UserViewModel
                {
                    FirstName = (user as User)?.FirstName, // Casts user to a custom User model to access FirstName.
                    Email = user.Email, // Fetches user's email.
                    Role = roles.FirstOrDefault() // Gets the first assigned role or null if none.
                });
            }

            return View(userList); // Passes the user list to the view.
        }
    }

    // ViewModel to represent user data for the view.
    public class UserViewModel
    {
        public string FirstName { get; set; } // User's first name.
        public string Email { get; set; } // User's email address.
        public string Role { get; set; } // User's role.
    }
}
