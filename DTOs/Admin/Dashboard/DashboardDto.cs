namespace AurHER.DTOs.Admin
{
    public class AdminDashboardDto
    {
        // Overview
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int NewOrdersToday { get; set; }
        public decimal RevenueToday { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Orders by status
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        // Inventory
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int InactiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }

        // Catalogue
        public int TotalCategories { get; set; }
        public int TotalCollections { get; set; }

        // Recent orders
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
    }
}