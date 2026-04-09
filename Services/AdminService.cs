using AurHER.DTOs.Admin;
using AurHER.Models.Enums;
using AurHER.Repositories.Interfaces;
using AurHER.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AurHER.Data;

namespace AurHER.Services
{
    public class AdminService : IAdminService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICollectionRepository _collectionRepository;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AdminService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            ICollectionRepository collectionRepository,
            AppDbContext context,  IConfiguration config)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _collectionRepository = collectionRepository;
            _context = context;
           _config = config;
        }

        public async Task<LoginResultDto> LoginAsync(LoginDto login)
        {
             if (string.IsNullOrWhiteSpace(login.UserName))
            {
                return new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Username cannot be empty"
                };
            }

            if (string.IsNullOrWhiteSpace(login.PassWord))
            {
                return  new LoginResultDto
                {
                    Success = false,
                    ErrorMessage = "Password cannot be empty"
                };
            }
            
             var adminUsername = _config["AdminCredentials:Username"];
            var adminPassword = _config["AdminCredentials:Password"];

             if (login.UserName != adminUsername || login.PassWord != adminPassword)
            {
                 return  new LoginResultDto
                {
                    Success = false,
                   ErrorMessage = "Invalid  UserName or Password"
                };
            }


            return  new LoginResultDto
           {
              Success = true,
              Role = "Admin",
             UserName = login.UserName 
           };
            


        }
        public async Task<AdminDashboardDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Fetch all orders
            var allOrders = await _orderRepository.GetAllAsync();
            var ordersList = allOrders.ToList();

            // Fetch all products with variants
            var allProducts = await _productRepository.GetAllWithDetailsAsync();
            var productsList = allProducts.ToList();

            // Fetch all variants for stock checking
            var allVariants = productsList
                .SelectMany(p => p.Variants)
                .ToList();

            // Recent 10 orders
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.CustomerName,
                    Email = o.Email,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ConfirmationCode = o.ConfirmationCode,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return new AdminDashboardDto
            {
                // Overview
                TotalOrders = ordersList.Count,
                TotalRevenue = ordersList
                    .Where(o => o.Status != OrderStatus.Cancelled)
                    .Sum(o => o.TotalAmount),
                NewOrdersToday = ordersList
                    .Count(o => o.CreatedAt.Date == today),
                RevenueToday = ordersList
                    .Where(o => o.CreatedAt.Date == today && o.Status != OrderStatus.Cancelled)
                    .Sum(o => o.TotalAmount),
                RevenueThisMonth = ordersList
                    .Where(o => o.CreatedAt >= startOfMonth && o.Status != OrderStatus.Cancelled)
                    .Sum(o => o.TotalAmount),

                // Orders by status
                PendingOrders = ordersList.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = ordersList.Count(o => o.Status == OrderStatus.Processing),
                ShippedOrders = ordersList.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredOrders = ordersList.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = ordersList.Count(o => o.Status == OrderStatus.Cancelled),

                // Inventory
                TotalProducts = productsList.Count,
                ActiveProducts = productsList.Count(p => p.IsActive),
                InactiveProducts = productsList.Count(p => !p.IsActive),
                
                LowStockCount = allVariants.Count(v => v.StockQuantity > 0 && v.StockQuantity <= 5),
                OutOfStockCount = allVariants.Count(v => v.StockQuantity == 0),

                // Catalogue
                TotalCategories = (await _categoryRepository.GetAllAsync()).Count(),
                TotalCollections = (await _collectionRepository.GetAllAsync()).Count(),

                // Recent orders
                RecentOrders = recentOrders
            };
        }
    }
}