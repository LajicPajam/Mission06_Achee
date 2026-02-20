using Mission06LajicPajam.Data;

var builder = WebApplication.CreateBuilder(args);

// MVC with Razor views.
builder.Services.AddControllersWithViews();
// Repository is scoped per web request.
builder.Services.AddScoped<IMovieRepository, SqliteMovieRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // Production-safe error handling and HSTS.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Standard ASP.NET Core middleware pipeline.
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Map static assets and conventional controller routes.
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
