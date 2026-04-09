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


    var builder = WebApplication.CreateBuilder(args);



    //local db connection string
    var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");

    // Check if running on Render (uses DATABASE_URL)
    var databaseUrl = builder.Configuration["DATABASE_URL"];

    if (!string.IsNullOrEmpty(databaseUrl))
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');

        // Handle case when port is not specified (returns -1)
        int port = uri.Port;
        if (port <= 0)
        {
            port = 5432; // Default PostgreSQL port
        }

        var builderConn = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = port,  // Use the validated port
            Username = userInfo[0],
            Password = userInfo[1],
            Database = uri.AbsolutePath.Trim('/'),
            SslMode = Npgsql.SslMode.Require,
            TrustServerCertificate = true
        };

        connectionString = builderConn.ToString();
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));



    //Paystack
    builder.Services.Configure<PaystackSettings>(builder.Configuration.GetSection("PaystackSettings"));

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

    builder.Services.AddScoped<IPaystackService, PaystackService>();
    builder.Services.AddHttpClient<IPaystackService, PaystackService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ICleanupService, CleanupService>();
    builder.Services.AddHostedService<CleanupBackgroundService>();
    builder.Services.AddScoped<IDeliveryLocationService, DeliveryLocationService>();

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


    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Migration failed: " + ex.Message);
        }
        
    }
    // Error handling
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
        app.UseHttpsRedirection();
    }

    // Middleware order are IMPORTANT 


    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
    app.Urls.Add($"http://0.0.0.0:{port}");

    app.Run();
