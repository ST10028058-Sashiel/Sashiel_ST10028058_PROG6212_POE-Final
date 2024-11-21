using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Sashiel_ST10028058_PROG6212_Part2.Data;


namespace Sashiel_ST10028058_PROG6212_Part2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure services
            ConfigureServices(builder);

            var app = builder.Build();

            // Configure middleware
            ConfigureMiddleware(app);

            // Apply migrations and seed roles
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();

                try
                {
                    Console.WriteLine("Applying database migrations...");
                    context.Database.Migrate();

                    Console.WriteLine("Seeding roles...");
                    SeedRoles(services).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during startup: {ex.Message}");
                }
            }

            // Start the application
            app.Run();
        }

        private static void ConfigureServices(WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddControllersWithViews();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                Console.WriteLine("Running in Development Environment");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                Console.WriteLine("Running in Production Environment");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();
        }

        private static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Lecturer", "Manager", "Co-ordinator", "HR" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    Console.WriteLine($"Seeding role: {role}");
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
