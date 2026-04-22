namespace AurHER.DTOs.Store
{
    public class CartDto
    {
        public int CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public int TotalItems { get; set; }
    }

    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int VariantId { get; set; }
        public string ProductName { get; set; }
        public string? PrimaryImage { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;
        public int StockQuantity { get; set; }
    }

    public class AddToCartDto
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; } = 1;
    }

        public class AddToCartResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }