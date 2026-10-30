    using Microsoft.EntityFrameworkCore;
    using AurHER.Data;
    using AurHER.Models;
    using AurHER.Services.Interfaces;
    using AurHER.Services;
    using AurHER.Repositories.Interfaces;
    using AurHER.Repositories;
    using Microsoft.AspNetCore.Authentication.Cookies; 
    using Microsoft.AspNetCore.RateLimiting;
    using System.Threading.RateLimiting;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.AspNetCore.HttpOverrides;
    using  CloudinaryDotNet;
    using Microsoft.Extensions.Options;


DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});


// Database
// Accepts either a key/value Npgsql string or a postgres:// / postgresql:// URL (Supabase, Render).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not set. On Render add the env var ConnectionStrings__DefaultConnection.");

if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
    connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    var uri = new Uri(connectionString);
    // Split only on the first ':' and URL-decode, so passwords containing ':' or %40 etc. work
    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var port = uri.Port > 0 ? uri.Port : 5432;

    connectionString =
        $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}

// Supabase transaction pooler (port 6543) does not support prepared statements / connection reset
if (connectionString.Contains("Port=6543", StringComparison.OrdinalIgnoreCase) &&
    !connectionString.Contains("No Reset On Close", StringComparison.OrdinalIgnoreCase))
{
    connectionString = connectionString.TrimEnd(';') + ";No Reset On Close=true;Max Auto Prepare=0";
}

// Log host only (never the password) so Render logs show where we're connecting
try
{
    var csb = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
    Console.WriteLine($"[DB] Connecting to {csb.Host}:{csb.Port} db={csb.Database} user={csb.Username}");
}
catch (Exception ex)
{
    Console.WriteLine($"[DB] Connection string is malformed: {ex.Message}");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3)));

//Paystack
builder.Services.Configure<PaystackSettings>(builder.Configuration.GetSection("PaystackSettings"));
    //Cloudinary 
    builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

    builder.Services.AddSingleton<Cloudinary>(sp =>
    {
    var settings = sp.GetRequiredService<IOptions<CloudinarySettings>>().Value;
    var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
    });


// Register custom services
builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ICollectionService, CollectionService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IShopService, ShopService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<ICheckoutService, CheckoutService>();
    builder.Services.AddScoped<IOrderTrackingService, OrderTrackingService>();

    builder.Services.AddHttpClient<IPaystackService, PaystackService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ICleanupService, CleanupService>();
    builder.Services.AddHostedService<CleanupBackgroundService>();
    builder.Services.AddScoped<IDeliveryLocationService, DeliveryLocationService>();
    builder.Services.AddScoped<IImageCompressionService, ImageCompressionService>();

    // Register repositories
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
    builder.Services.AddScoped<ICartRepository, CartRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
    builder.Services.AddScoped<IDeliveryLocationRepository, DeliveryLocationRepository>();


    // Add MVC
    builder.Services.AddControllersWithViews();

    // Add Session
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(5);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Admin/Login";
            options.LogoutPath = "/Admin/Logout";
            options.AccessDeniedPath = "/Admin/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

    var app = builder.Build();

    app.MapHealthChecks("/health");

    // Opt-in: set RUN_MIGRATIONS=true on Render to create/upgrade tables in an empty Supabase DB
    if (string.Equals(Environment.GetEnvironmentVariable("RUN_MIGRATIONS"), "true", StringComparison.OrdinalIgnoreCase))
    {
        using var migrationScope = app.Services.CreateScope();
        var db = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Console.WriteLine("[DB] Migrations applied.");
    }


    // Opt-in demo data: SEED_DATA=true to insert, SEED_DATA=clear to remove it again
    var seedMode = Environment.GetEnvironmentVariable("SEED_DATA");
    if (!string.IsNullOrWhiteSpace(seedMode) && !seedMode.Equals("false", StringComparison.OrdinalIgnoreCase))
    {
        using var seedScope = app.Services.CreateScope();
        var seedDb = seedScope.ServiceProvider.GetRequiredService<AurHER.Data.AppDbContext>();
        var seedLogger = seedScope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");
        await AurHER.Data.DataSeeder.RunAsync(seedDb, seedMode, seedLogger);
    }

// Error handling
if (app.Environment.IsDevelopment())
    {
       app.UseDeveloperExceptionPage();

    }
    else
    {
        app.UseMiddleware<AurHER.Middleware.MaintenanceMiddleware>();
        app.UseHsts();
        app.UseHttpsRedirection();
    }


// Middleware order are IMPORTANT 


    app.UseForwardedHeaders();

    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    var hostport = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    app.Urls.Add($"http://0.0.0.0:{hostport}");

    app.Run();
