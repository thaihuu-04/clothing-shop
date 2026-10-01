using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using QA.Helpers;
using Microsoft.EntityFrameworkCore;
using QA.Models;
using QA.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

// =========================
// MVC
// =========================
builder.Services.AddControllersWithViews();

// Xuất tiếng Việt trực tiếp trong HTML thay vì mã hóa thành &#x...;
builder.Services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

// =========================
// SESSION
// =========================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// =========================
// DATABASE
// =========================
builder.Services.AddDbContext<ClothingStoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// =========================
// COOKIE AUTHENTICATION
// =========================
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // Đăng xuất ngay nếu tài khoản bị khóa hoặc bị đổi quyền
        options.Events.OnValidatePrincipal = async context =>
        {
            var principal = context.Principal;
            int userId = principal?.GetUserId() ?? 0;
            string? roleId = principal?.FindFirst("RoleId")?.Value;

            var db = context.HttpContext.RequestServices.GetRequiredService<ClothingStoreDbContext>();
            var user = await db.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => new { u.IsActive, u.RoleId })
                .FirstOrDefaultAsync();

            if (user == null || !user.IsActive || user.RoleId.ToString() != roleId)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

builder.Services.AddAuthorization();

// =========================
// APP SERVICES
// =========================
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IImageService, ImageService>();


// =========================
// BUILD APP
// =========================

var app = builder.Build();


// =========================
// ERROR
// =========================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// =========================
// HTTPS
// =========================

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

app.UseHttpsRedirection();


// =========================
// STATIC FILES
// =========================

app.UseStaticFiles();


// =========================
// ROUTING
// =========================

app.UseRouting();


// =========================
// SESSION
// =========================

app.UseSession();


// =========================
// AUTHENTICATION
// =========================

app.UseAuthentication();


// =========================
// AUTHORIZATION
// =========================

app.UseAuthorization();


// =========================
// ADMIN AREA
// =========================

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);


// =========================
// DEFAULT
// =========================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);


app.Run();