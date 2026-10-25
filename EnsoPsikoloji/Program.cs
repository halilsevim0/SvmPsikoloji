using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EnsoPsikoloji.Models;
using EnsoPsikoloji.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext Servisi
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC ve Razor Pages Servisleri
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Bildirim Servisleri (E-Posta & SMS)
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ISmsService, SmsService>();

// Identity ve Rol Servisleri
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // Identity hazir arayuzleri icin gereklidir

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kimlik dogrulama ve yetkilendirme (Siralama onemlidir)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Identity sayfalarinin calismasi icin yonlendirme
app.MapRazorPages();

// Otomatik Rol ve Admin Kullanici Tohumlama (Seeding)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        await DbInitializer.SeedRolesAndAdminAsync(services);
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Veritabani tohumlama (Seed) sirasinda baglanti hatasi olustu.");
}

app.Run();
