using LogiTrack.Data;
using LogiTrack.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext (SQLite)
builder.Services.AddDbContext<LogiTrackContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") 
        ?? "Data Source=logitrack.db"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<LogiTrackContext>()
.AddDefaultTokenProviders();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-change-me";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LogiTrack";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LogiTrackAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // enable https in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// MVC + MemoryCache
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseRouting();

// AuthN/AuthZ
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed a Manager role (optional simple seed)
/* using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    var roleName = "Manager";
    if (!await roleManager.RoleExistsAsync(roleName))
        await roleManager.CreateAsync(new IdentityRole(roleName));
    // Optionally seed an admin user from config
    var adminEmail = builder.Configuration["Seed:AppAdmin@LogiTrack.Com"];
    var adminPassword = builder.Configuration["Seed:app@dmin"];
    if (!string.IsNullOrEmpty(adminEmail) && !string.IsNullOrEmpty(adminPassword))
    {
        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing == null)
        {
            var user = new ApplicationUser { UserName = adminEmail, Email = adminEmail };
            var create = await userManager.CreateAsync(user, adminPassword);
            if (create.Succeeded)
                await userManager.AddToRoleAsync(user, roleName);
        }
    }
} */

app.Run();
