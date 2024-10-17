using Microsoft.AspNetCore.Identity;

namespace Sashiel_ST10028058_PROG6212_Part2.Models
{

        public class User : IdentityUser
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

        }

    
}
