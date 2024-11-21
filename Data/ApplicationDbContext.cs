using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sashiel_ST10028058_PROG6212_Part2.Models;

namespace Sashiel_ST10028058_PROG6212_Part2.Data
{
    // ApplicationDbContext serves as the bridge between the application and the database.
    // It inherits from IdentityDbContext to include Identity framework functionalities for authentication and authorization.
    public class ApplicationDbContext : IdentityDbContext
    {
        // Constructor to configure the ApplicationDbContext with specific options.
        // It passes the options to the base class (IdentityDbContext).
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet for the 'User' model. This represents the 'Users' table in the database.
        // The Identity framework will use this to manage application users.
        public DbSet<User> User { get; set; }

        // DbSet for the 'Claim' model. This represents the 'Claims' table in the database.
        // It will handle all claims-related operations through Entity Framework.
        public DbSet<Claims> Claims { get; set; }
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