using Microsoft.AspNetCore.Identity;

namespace Sashiel_ST10028058_PROG6212_Part2.Models
{
    // The User class represents an application user and extends IdentityUser 
    // to include additional properties specific to the application's requirements.
    public class User : IdentityUser
    {
        // Stores the first name of the user.
        public string FirstName { get; set; }

        // Stores the last name of the user.
        public string LastName { get; set; }
    }
}
//# Assistance provided by ChatGPT
//# Code and support generated with the help of OpenAI's ChatGPT.
// code attribution
// W3schools
//https://www.w3schools.com/cs/index.php

// code attribution
//Bootswatch
//https://bootswatch.com/

// code attribution
// https://learn.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/start-mvc?view=aspnetcore-8.0&tabs=visual-studio

// code attribution
// https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=visual-studio

// code attribution
// https://youtu.be/qvsWwwq2ynE?si=vwx2O4bCAFDFh5m_