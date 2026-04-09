namespace AurHER.Services
{
    public static class EmailTemplates
    {
        private static string BaseTemplate(string content) => $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8' />
            <style>
                body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background: #F8E8EE; margin: 0; padding: 20px; }}
                .container {{ max-width: 560px; margin: 0 auto; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 20px rgba(0,0,0,0.08); }}
                .header {{ background: #0F172A; padding: 28px 32px; text-align: center; }}
                .brand {{ font-size: 1.8rem; font-weight: 800; }}
                .brand .aur {{ color: #C9A84C; }}
                .brand .her {{ color: #C8145A; }}
                .body {{ padding: 32px; }}
                .title {{ font-size: 1.1rem; font-weight: 700; color: #0F172A; margin-bottom: 12px; }}
                .text {{ font-size: 0.9rem; color: #4A5568; line-height: 1.7; margin-bottom: 16px; }}
                .badge {{ display: inline-block; background: #FAE8EF; color: #C8145A; padding: 6px 16px; border-radius: 20px; font-weight: 700; font-size: 0.9rem; margin: 8px 0; }}
                .divider {{ border: none; border-top: 1px solid #F0E8EC; margin: 20px 0; }}
                .footer {{ background: #F8E8EE; padding: 20px 32px; text-align: center; font-size: 0.78rem; color: #94A3B8; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <div class='brand'><span class='aur'>Aur</span><span class='her'>HER</span></div>
                </div>
                <div class='body'>
                    {content}
                </div>
                <div class='footer'>
                    © {DateTime.Now.Year} AurHER. All rights reserved.<br/>
                    This is an automated notification.
                </div>
            </div>
        </body>
        </html>";

        public static string NewOrder(string customerName, decimal total, string confirmationCode) =>
            BaseTemplate($@"
                <div class='title'>🛍️ New Order Received!</div>
                <p class='text'>A new order has just been placed on your store.</p>
                <hr class='divider'/>
                <p class='text'><strong>Customer:</strong> {customerName}</p>
                <p class='text'><strong>Order Total:</strong> ₦{total:N0}</p>
                <p class='text'><strong>Confirmation Code:</strong></p>
                <div class='badge'>{confirmationCode}</div>
                <hr class='divider'/>
                <p class='text'>Log in to your admin dashboard to process this order.</p>
            ");

        public static string LowStock(string productName, string sku, int quantity) =>
            BaseTemplate($@"
                <div class='title'>⚠️ Low Stock Alert</div>
                <p class='text'>A product variant is running low on stock.</p>
                <hr class='divider'/>
                <p class='text'><strong>Product:</strong> {productName}</p>
                <p class='text'><strong>SKU:</strong> {sku}</p>
                <p class='text'><strong>Remaining Stock:</strong></p>
                <div class='badge'>{quantity} units left</div>
                <hr class='divider'/>
                <p class='text'>Please restock soon to avoid running out.</p>
            ");

        public static string OutOfStock(string productName, string sku) =>
            BaseTemplate($@"
                <div class='title'>🚨 Out of Stock!</div>
                <p class='text'>A product variant has run out of stock.</p>
                <hr class='divider'/>
                <p class='text'><strong>Product:</strong> {productName}</p>
                <p class='text'><strong>SKU:</strong> {sku}</p>
                <hr class='divider'/>
                <p class='text' style='color:#EF4444;'><strong>This product is no longer available to customers.</strong></p>
                <p class='text'>Please restock immediately.</p>
            ");

        public static string OrderStatusChanged(string customerName, string confirmationCode, string newStatus) =>
            BaseTemplate($@"
                <div class='title'>Your Order has been {newStatus}!</div>
                <p class='text'>Hi {customerName}, your AurHER order status has been updated.</p>
                <hr class='divider'/>
                <p class='text'><strong>Order Code:</strong></p>
                <div class='badge'>{confirmationCode}</div>
                <p class='text'><strong>New Status:</strong> {newStatus}</p>
                <hr class='divider'/>
                <p class='text'>You can track your order at any time using your confirmation code on our website.</p>
                <p class='text'>Thank you for shopping with AurHER! 💕</p>
            ");

        public static string OrderConfirmation(string customerName, decimal total, string confirmationCode) =>
             BaseTemplate($@"
        <div class='title'>Order Confirmed! 🎉</div>
        <p class='text'>Hi {customerName}, thank you for shopping with AurHER!</p>
        <p class='text'>Your order has been received and we're getting it ready for you.</p>
        <hr class='divider'/>
        <p class='text'><strong>Order Total:</strong> ₦{total:N0}</p>
        <p class='text'><strong>Your Confirmation Code:</strong></p>
        <div class='badge'>{confirmationCode}</div>
        <hr class='divider'/>
        <p class='text'>Track your order anytime at <strong>aurher.com/Track/Index</strong> using your confirmation code.</p>
        <p class='text' style='color:#C8145A;'>💕 Thank you for choosing AurHER!</p>
    ");
    }
}