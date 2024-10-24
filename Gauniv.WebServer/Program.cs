using Gauniv.WebServer.Data;
using Gauniv.WebServer.Dtos;
using Gauniv.WebServer.Security;
using Gauniv.WebServer.Services;
using Gauniv.WebServer.Websocket;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Configuration pour définir la culture
var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Connexion à la base de données PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configuration des services Identity pour la gestion des utilisateurs
builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;  // Exigence minimale pour le mot de passe
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>() // Gestion des rôles si nécessaire
.AddEntityFrameworkStores<ApplicationDbContext>();

// Ajout des services pour les pages Razor, MVC et SignalR
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// Ajout des services d'hébergement (ex : Redis, services en arrière-plan)
builder.Services.AddHostedService<OnlineService>(); // Gestion des utilisateurs en ligne avec SignalR
builder.Services.AddHostedService<SetupService>();  // Migration ou configuration initiale

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // Set the limit to 100 MB or any size you need
});

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 104857600; // Set the limit to 100 MB or any size you need
    await next.Invoke();
});
// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();  // Utilisation des migrations automatiques
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Ajouter dans le fichier Program.cs avant `app.Build()` :


app.UseHttpsRedirection();  // Redirection vers HTTPS
app.UseStaticFiles();       // Utilisation des fichiers statiques (CSS, JS)

app.UseRouting();           // Configuration du routage

app.UseAuthentication();    // Middleware pour l'authentification
app.UseAuthorization();     // Middleware pour l'autorisation

// Configuration des routes pour les contrôleurs et les pages Razor
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();



// Configuration des hubs SignalR (pour la gestion des utilisateurs en ligne)
app.MapHub<OnlineHub>("/online");

// Call the CreateRoles method within a scope
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateRoles(services);
}

app.Run();

async Task CreateRoles(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roleNames = { "Admin", "User" }; // Example roles
    IdentityResult roleResult;

    foreach (var roleName in roleNames)
    {
        var roleExist = await roleManager.RoleExistsAsync(roleName);
        if (!roleExist)
        {
            // Create the roles and seed them to the database
            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}
