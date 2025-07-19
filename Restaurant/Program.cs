using Restaurant.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register services before building the app
builder.Services.AddControllersWithViews();

// ✅ Register DbContext
builder.Services.AddDbContext<RmsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RMSDatabase")));

// ✅ Register session support
builder.Services.AddSession();

var app = builder.Build();

// ✅ Middleware configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Make sure this is after UseRouting

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
