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


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});


// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration
        .GetConnectionString("DefaultConnection")));



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


// Error handling
if (app.Environment.IsDevelopment())
    {
       app.UseDeveloperExceptionPage();

    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
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
