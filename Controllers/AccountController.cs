using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Sashiel_ST10028058_PROG6212_Part2.Models; // Update with your namespace if different

namespace Sashiel_ST10028058_PROG6212_Part2.Areas.Identity.Pages.Account
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Account/Index
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            var userList = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserViewModel
                {
                    FirstName = (user as User)?.FirstName, // Assuming FirstName is in your custom User class
                    Email = user.Email,
                    Role = roles.FirstOrDefault() // Assuming each user has a single role
                });
            }

            return View(userList);
        }
    }

    public class UserViewModel
    {
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}