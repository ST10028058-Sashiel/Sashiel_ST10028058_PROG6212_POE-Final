using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sashiel_ST10028058_PROG6212_Part2.Data;

namespace Sashiel_ST10028058_PROG6212_Part2
{
    // Entry point of the application
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create a WebApplicationBuilder to configure services and middleware
            var builder = WebApplication.CreateBuilder(args);

            // Retrieve the connection string from configuration settings
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            // Register the ApplicationDbContext with SQL Server configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Enable detailed exception pages for database errors
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Configure Identity with password rules and role management
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.Password.RequireDigit = true; // Require at least one numeric character
                options.Password.RequireLowercase = true; // Require at least one lowercase letter
                options.Password.RequiredLength = 6; // Set minimum password length
            })
                .AddRoles<IdentityRole>() // Enable role management
                .AddEntityFrameworkStores<ApplicationDbContext>() // Store Identity data in the database
                .AddDefaultTokenProviders(); // Add token providers for password resets, etc.

            // Add support for controllers with views
            builder.Services.AddControllersWithViews();

            // Configure logging to use the console
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Build the application with configured services
            var app = builder.Build();

            // Configure the middleware pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Show detailed error pages in development
                app.UseMigrationsEndPoint(); // Apply migrations at runtime
            }
            else
            {
                app.UseExceptionHandler("/Home/Error"); // Use custom error handling in production
                app.UseHsts(); // Enforce HTTPS in production
            }

            // Enforce HTTPS redirection and serve static files (like images, CSS)
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting(); // Enable endpoint routing

            // Add Authentication and Authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            // Define the default controller route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Map Identity UI Razor Pages (e.g., Login, Register)
            app.MapRazorPages();

            // Ensure the database is migrated and seed roles at startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate(); // Apply pending migrations
                SeedRoles(services).Wait(); // Seed roles asynchronously
            }

            // Start the application
            app.Run();
        }

        // Method to seed roles into the system if they don't exist
        private static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Define the roles to be seeded
            string[] roles = { "Lecturer", "Manager", "Co-ordinator" };

            // Check and create each role if it doesn't exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
