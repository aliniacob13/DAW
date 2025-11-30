using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;                       // schimbă dacă ai alt namespace
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection string (din appsettings.json sau din variabile de mediu)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// 2. Înregistrare DbContext pe MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 4)),   // versiunea de MySQL din docker
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    )
);

// 3. MVC (controllers + views)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();

// fișiere statice din wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 5. Rute MVC
app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();