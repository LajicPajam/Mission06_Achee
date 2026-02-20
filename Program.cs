using Mission06LajicPajam.Data;

var builder = WebApplication.CreateBuilder(args);

// Register MVC so controllers can return Razor views.
builder.Services.AddControllersWithViews();
// Create one repository instance per request to keep database access isolated.
builder.Services.AddScoped<IMovieRepository, SqliteMovieRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // In production, show a friendly error page and force HTTPS.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Core request pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Serve CSS/JS and map the default controller/action route.
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
