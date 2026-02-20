using Mission06LajicPajam.Data;

var builder = WebApplication.CreateBuilder(args);

// Register MVC so controllers can return Razor views.
builder.Services.AddControllersWithViews();
// Keep one repository instance for the app lifetime in Mission 06.
builder.Services.AddSingleton<IMovieRepository, SqliteMovieRepository>();

var app = builder.Build();

// Initialize and seed the SQLite database once at startup.
using (var scope = app.Services.CreateScope())
{
    var repo = scope.ServiceProvider.GetRequiredService<IMovieRepository>();
    repo.InitializeDatabase();
    repo.SeedFavoriteMovies();
}

if (!app.Environment.IsDevelopment())
{
    // In production, use a friendly error page and enforce HTTPS.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Core middleware for routing and authorization.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Serve static files and map the default route.
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
