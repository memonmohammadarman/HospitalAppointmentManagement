// Import MVC types used to configure response caching.
using Microsoft.AspNetCore.Mvc;

// Create the application builder using the command-line arguments.
var builder = WebApplication.CreateBuilder(args);

// Register MVC controllers and Razor Views in the dependency injection container.
builder.Services.AddControllersWithViews();

// Register an in-memory distributed cache required by ASP.NET Core Session.
builder.Services.AddDistributedMemoryCache();

// Register IMemoryCache for caching the doctor list.
builder.Services.AddMemoryCache();

// Register ASP.NET Core Response Caching services.
builder.Services.AddResponseCaching();

// Register ASP.NET Core Session services.
builder.Services.AddSession(options =>
{
    // Make the Session cookie essential for the application.
    options.Cookie.IsEssential = true;

    // Give the Session cookie a specific name.
    options.Cookie.Name = ".HospitalAppointment.Session";

    // Keep the Session active for 20 minutes after the last request.
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

// Register HttpClientFactory for making asynchronous web requests.
builder.Services.AddHttpClient("JsonPlaceholder", client =>
{
    // Set the base URL for the JSONPlaceholder web service.
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
});

// Build the web application.
var app = builder.Build();

// Check whether the application is running outside Development mode.
if (!app.Environment.IsDevelopment())
{
    // Use the standard exception handler in non-development environments.
    app.UseExceptionHandler("/Home/Error");

    // Enable HTTP Strict Transport Security in non-development environments.
    app.UseHsts();
}

// Redirect HTTP requests to HTTPS when an HTTPS endpoint is available.
app.UseHttpsRedirection();

// Enable CSS, JavaScript and other static files from wwwroot.
app.UseStaticFiles();

// Enable ASP.NET Core routing.
app.UseRouting();

// Enable response caching middleware.
app.UseResponseCaching();

// Enable Session middleware.
app.UseSession();

// Enable authorization middleware.
app.UseAuthorization();

// Configure the default MVC route.
app.MapControllerRoute(
    // Give the route a name.
    name: "default",

    // Define controller, action and optional ID routing.
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Start the ASP.NET Core application.
app.Run();