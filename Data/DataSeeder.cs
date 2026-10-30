using AurHER.Models;
using AurHER.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace AurHER.Data
{
    /// <summary>
    /// Mock data for demos. Opt-in:  SEED_DATA=true   -> insert (safe to re-run, skips if already seeded)
    ///                               SEED_DATA=clear  -> remove only the data this seeder created
    /// Seeded rows are recognisable: SKUs start with "SEED-", order e-mails end with "@seed.aurher.test".
    /// </summary>
    public static class DataSeeder
    {
        private const string SkuPrefix = "SEED-";
        private const string EmailDomain = "@seed.aurher.test";

        public static async Task RunAsync(AppDbContext db, string mode, ILogger logger)
        {
            if (mode.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                await ClearAsync(db, logger);
                return;
            }

            if (await db.ProductVariants.AnyAsync(v => v.SKU.StartsWith(SkuPrefix)))
            {
                logger.LogInformation("[Seed] Already seeded, skipping.");
                return;
            }

            var rng = new Random(2026); // fixed seed = same data every time

            // ── Delivery locations ──
            var locations = new (string Name, decimal Fee)[]
            {
                ("Ibadan - Within City", 1500m), ("Ibadan - Outskirts", 2500m),
                ("Lagos Island", 3500m), ("Lagos Mainland", 3000m), ("Abuja", 5000m),
            };
            foreach (var l in locations)
                if (!await db.DeliveryLocations.AnyAsync(d => d.Name == l.Name))
                    db.DeliveryLocations.Add(new DeliveryLocation { Name = l.Name, Fee = l.Fee });

            // ── Categories ──
            var cats = new Dictionary<string, Category>
            {
                ["Dresses"] = new() { Name = "Dresses (Seed)", Description = "Elegant dresses for every occasion" },
                ["Tops"] = new() { Name = "Tops (Seed)", Description = "Blouses, tees and crop tops" },
                ["Skirts"] = new() { Name = "Skirts (Seed)", Description = "Mini, midi and maxi skirts" },
                ["Ankara"] = new() { Name = "Ankara (Seed)", Description = "Bold African prints" },
                ["Accessories"] = new() { Name = "Accessories (Seed)", Description = "Bags, scarves and jewellery" },
            };
            db.Categories.AddRange(cats.Values);
            await db.SaveChangesAsync();

            // ── Products: (name, category, description, base price, sizes, colors) ──
            var defs = new (string Name, string Cat, string Desc, decimal Price, string[] Sizes, string[] Colors)[]
            {
                ("Aura Wrap Dress", "Dresses", "Flowing wrap dress with a flattering waist tie.", 18500m, new[]{"S","M","L"}, new[]{"Rose","Black"}),
                ("Zuri Maxi Dress", "Dresses", "Floor-length maxi in soft breathable fabric.", 24000m, new[]{"M","L","XL"}, new[]{"Emerald","Ivory"}),
                ("Nia Bodycon Dress", "Dresses", "Stretch bodycon for evenings out.", 15500m, new[]{"S","M","L"}, new[]{"Red","Black"}),
                ("Sade Satin Blouse", "Tops", "Smooth satin blouse, office to dinner.", 9500m, new[]{"S","M","L"}, new[]{"Champagne","Navy"}),
                ("Lola Crop Top", "Tops", "Casual cotton crop top.", 6000m, new[]{"S","M"}, new[]{"White","Pink"}),
                ("Ife Ruffle Top", "Tops", "Ruffled sleeves with a relaxed fit.", 8200m, new[]{"M","L"}, new[]{"Peach","White"}),
                ("Kemi Pleated Midi Skirt", "Skirts", "Pleated midi that moves beautifully.", 11000m, new[]{"S","M","L"}, new[]{"Olive","Black"}),
                ("Tola Denim Mini Skirt", "Skirts", "Classic high-waisted denim mini.", 8800m, new[]{"S","M","L"}, new[]{"Blue"}),
                ("Adaeze Ankara Two-Piece", "Ankara", "Matching Ankara top and skirt set.", 27500m, new[]{"M","L","XL"}, new[]{"Gold Print","Blue Print"}),
                ("Yetunde Ankara Gown", "Ankara", "Statement Ankara gown, tailored fit.", 32000m, new[]{"M","L"}, new[]{"Sunset Print"}),
                ("Amara Beaded Necklace", "Accessories", "Handmade beaded statement necklace.", 4500m, new[]{"One Size"}, new[]{"Gold","Multi"}),
                ("Ore Leather Tote", "Accessories", "Roomy faux-leather tote bag.", 16500m, new[]{"One Size"}, new[]{"Tan","Black"}),
            };

            var products = new List<Product>();
            var createdBase = DateTime.UtcNow.AddDays(-60);
            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];
                var p = new Product
                {
                    Name = d.Name,
                    Description = d.Desc,
                    CategoryId = cats[d.Cat].Id,
                    IsActive = true,
                    CreatedAt = createdBase.AddDays(i * 2),
                    Variants = new List<ProductVariant>(),
                    Images = new List<ProductImage>
                    {
                        new() { ImageUrl = $"https://placehold.co/600x800/be185d/ffffff?text={Uri.EscapeDataString(d.Name)}", Isprimary = true },
                        new() { ImageUrl = $"https://placehold.co/600x800/1f2937/ffffff?text={Uri.EscapeDataString(d.Name)}+2", Isprimary = false },
                    }
                };
                int n = 0;
                foreach (var size in d.Sizes)
                    foreach (var color in d.Colors)
                    {
                        n++;
                        p.Variants.Add(new ProductVariant
                        {
                            Size = size,
                            Color = color,
                            Price = d.Price,
                            StockQuantity = 40 + rng.Next(0, 30),
                            ReservedStock = 0,
                            SKU = $"{SkuPrefix}{i + 1:D2}-{n:D2}"
                        });
                    }
                products.Add(p);
            }
            db.Products.AddRange(products);
            await db.SaveChangesAsync();

            // ── Collections ──
            var collections = new[]
            {
                new Collection { Name = "New Arrivals (Seed)", Products = products.TakeLast(5).Select(p => new CollectionProduct { ProductId = p.Id }).ToList() },
                new Collection { Name = "Owambe Season (Seed)", Products = products.Where(p => p.CategoryId == cats["Ankara"].Id || p.CategoryId == cats["Dresses"].Id).Select(p => new CollectionProduct { ProductId = p.Id }).ToList() },
                new Collection { Name = "Everyday Essentials (Seed)", Products = products.Where(p => p.CategoryId == cats["Tops"].Id || p.CategoryId == cats["Skirts"].Id).Select(p => new CollectionProduct { ProductId = p.Id }).ToList() },
            };
            db.Collections.AddRange(collections);
            await db.SaveChangesAsync();

            // ── Orders over the last 45 days ──
            var customers = new (string Name, string Phone)[]
            {
                ("Ngozi Adeyemi","08031230001"),("Funke Balogun","08031230002"),("Chiamaka Obi","08031230003"),
                ("Bisi Alade","08031230004"),("Hauwa Musa","08031230005"),("Temitope Ojo","08031230006"),
                ("Blessing Eze","08031230007"),("Zainab Yusuf","08031230008"),("Damilola Ayo","08031230009"),
                ("Ifeoma Nwosu","08031230010"),
            };
            var variants = products.SelectMany(p => p.Variants).ToList();
            var locNames = locations.Select(l => l.Name).ToArray();
            var dbLocations = await db.DeliveryLocations.Where(d => locNames.Contains(d.Name)).ToListAsync();

            const int orderCount = 45;
            for (int i = 0; i < orderCount; i++)
            {
                var c = customers[rng.Next(customers.Length)];
                var loc = dbLocations[rng.Next(dbLocations.Count)];
                var created = DateTime.UtcNow.AddDays(-rng.Next(0, 45)).AddHours(-rng.Next(0, 24)).AddMinutes(-rng.Next(0, 60));
                var age = (DateTime.UtcNow - created).TotalDays;

                // Older orders are more likely to be finished
                OrderStatus status;
                var roll = rng.Next(100);
                if (age < 2) status = roll < 60 ? OrderStatus.Pending : OrderStatus.Processing;
                else if (age < 7) status = roll < 25 ? OrderStatus.Processing : roll < 75 ? OrderStatus.Shipped : OrderStatus.Delivered;
                else status = roll < 8 ? OrderStatus.Cancelled : OrderStatus.Delivered;

                var order = new Order
                {
                    CustomerName = c.Name,
                    Email = $"{c.Name.ToLower().Replace(' ', '.')}{EmailDomain}",
                    Phone = c.Phone,
                    ShippingAddress = $"{rng.Next(1, 99)} Sample Street, {loc.Name}",
                    DeliveryLocation = loc.Name,
                    DeliveryFee = loc.Fee,
                    Status = status,
                    CreatedAt = created,
                    Items = new List<OrderItem>()
                };

                decimal subtotal = 0;
                foreach (var v in variants.OrderBy(_ => rng.Next()).Take(rng.Next(1, 4)))
                {
                    var qty = rng.Next(1, 3);
                    order.Items.Add(new OrderItem { ProductVariantId = v.Id, Quantity = qty, Price = v.Price });
                    subtotal += v.Price * qty;
                    if (status != OrderStatus.Cancelled) v.StockQuantity = Math.Max(0, v.StockQuantity - qty);
                }
                order.TotalAmount = subtotal + loc.Fee;

                order.Payment = new Payment
                {
                    Amount = order.TotalAmount,
                    PaymentMethod = "Paystack",
                    TransactionReference = $"SEED-{Guid.NewGuid():N}".Substring(0, 20),
                    Status = status == OrderStatus.Pending ? PaymentStatus.Pending
                           : status == OrderStatus.Cancelled ? PaymentStatus.Failed
                           : PaymentStatus.Completed,
                    PaidAt = status is OrderStatus.Pending or OrderStatus.Cancelled ? null : created.AddMinutes(3)
                };

                db.Orders.Add(order);
            }
            await db.SaveChangesAsync();
            logger.LogInformation("[Seed] Done: {P} products, {O} orders.", products.Count, orderCount);
        }

        private static async Task ClearAsync(AppDbContext db, ILogger logger)
        {
            var orders = await db.Orders.Include(o => o.Items).Include(o => o.Payment)
                .Where(o => o.Email.EndsWith(EmailDomain)).ToListAsync();
            db.Payments.RemoveRange(orders.Where(o => o.Payment != null).Select(o => o.Payment!));
            db.OrderItems.RemoveRange(orders.SelectMany(o => o.Items));
            db.Orders.RemoveRange(orders);

            var productIds = await db.ProductVariants.Where(v => v.SKU.StartsWith(SkuPrefix))
                .Select(v => v.ProductId).Distinct().ToListAsync();
            db.CartItems.RemoveRange(db.CartItems.Where(ci => ci.ProductVariant.SKU.StartsWith(SkuPrefix)));
            db.CollectionProducts.RemoveRange(db.CollectionProducts.Where(cp => productIds.Contains(cp.ProductId)));
            db.ProductImages.RemoveRange(db.ProductImages.Where(i => productIds.Contains(i.ProductId)));
            db.ProductVariants.RemoveRange(db.ProductVariants.Where(v => productIds.Contains(v.ProductId)));
            db.Products.RemoveRange(db.Products.Where(p => productIds.Contains(p.Id)));
            await db.SaveChangesAsync();

            db.Collections.RemoveRange(db.Collections.Where(c => c.Name.EndsWith("(Seed)")));
            db.Categories.RemoveRange(db.Categories.Where(c => c.Name.EndsWith("(Seed)")));
            await db.SaveChangesAsync();
            logger.LogInformation("[Seed] Seed data cleared.");
        }
    }
}
