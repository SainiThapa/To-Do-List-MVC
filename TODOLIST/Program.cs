using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TODOLIST.Data; // Ensure this matches your namespace for ApplicationDbContext
using TODOLIST.Models; // Ensure this matches your namespace for IdentityUser
using TODOLIST.Services; // Ensure this matches your namespace for TaskService

var builder = WebApplication.CreateBuilder(args);

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity services with role management
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register the TaskService as a scoped service
builder.Services.AddScoped<TaskService>();

// Add MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Custom error handling
    app.UseHsts(); // Add HTTP Strict Transport Security (HSTS)
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseStaticFiles(); // Enable serving static files

app.UseRouting(); // Add routing middleware

app.UseAuthentication(); // Ensure that authentication middleware is used
app.UseAuthorization(); // Ensure that authorization middleware is used

// Configure default route for controllers
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services); 
}

app.Run(); // Start the application

// Method to seed roles into the database
static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "User" }; // Define roles to seed
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        // Check if the role already exists
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Create the role if it doesn't exist
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
