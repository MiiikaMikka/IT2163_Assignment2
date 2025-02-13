using IT2163_Assignment2_234695G.Context;
using IT2163_Assignment2_234695G.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IT2163_Assignment2_234695G.Utils;
using Microsoft.AspNetCore.Identity.UI.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IEmailSender, EmailSender>();

// Register ApplicationDbContext with Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Set lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Automatically unlock account after 30 minutes
    options.Lockout.MaxFailedAccessAttempts = 5; // Max failed attempts before lockout
    options.Lockout.AllowedForNewUsers = true; // Enable lockout for new users

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;

    options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;

})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddPasswordValidator<PasswordHistoryValidator>();


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});     

builder.Services.AddDataProtection();

// Add services to the container.
builder.Services.AddRazorPages();

// Configure session
builder.Services.AddDistributedMemoryCache(); // In-memory cache for session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); // Set session timeout
    options.Cookie.HttpOnly = true; // Make session cookie HTTP-only for security
    options.Cookie.IsEssential = true; // Required for session to work
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSession();



app.MapRazorPages();

app.MapGet("/", async context =>
{
    var user = context.User;

    if (user.Identity.IsAuthenticated)
    {
        // Redirect authenticated users to the homepage
        context.Response.Redirect("/Index");
    }
    else     // If session has expired, redirect to login page
    if (context.Session.GetString("UserSession") == null)
    {
        context.Response.Redirect("/Login"); // Redirect to login page if session expired
        return;
    }
    else
    {
        // Redirect unauthenticated users to the login page
        context.Response.Redirect("/Logout");
    }

    await Task.CompletedTask;
});

app.Run();
