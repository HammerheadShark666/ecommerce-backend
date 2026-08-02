using ECommerce.Application.Constants;
using ECommerce.Domain.Entities.Product;
using ECommerce.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Seed_Data;

public static class DatabaseSeeder
{
    private const string SeedUser = "seed-script";

    private static readonly string[] ColorVariants = { "Black", "White", "Blue", "Red", "Grey" };
    private static readonly string[] BookFormats = { "Hardcover", "Paperback", "eBook", "Audiobook", "Large Print" };

    private sealed record ProductType(string BaseName, decimal Price);
    private sealed record CategorySeed(string Name, string[] Brands, string[] Variants, ProductType[] Types);

    private static readonly CategorySeed[] Catalogue =
    {
        new("Electronics", new[] { "Acme", "Nimbus", "Vertex" }, ColorVariants, new[]
        {
            new ProductType("Wireless Headphones", 199.99m), new ProductType("Bluetooth Speaker", 59.99m),
            new ProductType("Smart Watch", 149.99m), new ProductType("Fitness Band", 49.99m),
            new ProductType("Laptop", 899.99m), new ProductType("Ultrabook", 1099.99m),
            new ProductType("27-inch Monitor", 329.99m), new ProductType("24-inch Monitor", 219.99m),
            new ProductType("Mechanical Keyboard", 89.99m), new ProductType("Wireless Mouse", 34.99m),
            new ProductType("Gaming Mouse", 54.99m), new ProductType("USB-C Hub", 44.99m),
            new ProductType("Portable SSD", 109.99m), new ProductType("External HDD", 79.99m),
            new ProductType("Power Bank", 29.99m), new ProductType("Wireless Charger", 24.99m),
            new ProductType("Webcam", 44.99m), new ProductType("Ring Light", 34.99m),
            new ProductType("Action Camera", 249.99m), new ProductType("DSLR Camera", 649.99m),
            new ProductType("Camera Tripod", 39.99m), new ProductType("Home Theatre Soundbar", 179.99m),
            new ProductType("Wireless Earbuds", 69.99m), new ProductType("Noise Cancelling Earbuds", 159.99m),
            new ProductType("Streaming Stick", 39.99m), new ProductType("Smart Plug", 14.99m),
            new ProductType("Smart Bulb", 12.99m), new ProductType("Smart Thermostat", 129.99m),
            new ProductType("Robot Vacuum", 299.99m), new ProductType("Cordless Vacuum", 199.99m),
            new ProductType("Air Purifier", 149.99m), new ProductType("Electric Toothbrush", 49.99m),
            new ProductType("Hair Dryer", 39.99m), new ProductType("Hair Straightener", 44.99m),
            new ProductType("Electric Shaver", 59.99m), new ProductType("Wireless Router", 89.99m),
            new ProductType("Mesh Wi-Fi System", 179.99m), new ProductType("Network Switch", 34.99m),
            new ProductType("HDMI Cable", 9.99m), new ProductType("Graphics Tablet", 79.99m)
        }),

        new("Home & Kitchen", new[] { "Cascade", "Aurora" }, ColorVariants, new[]
        {
            new ProductType("Table Lamp", 34.99m), new ProductType("Floor Lamp", 59.99m),
            new ProductType("Desk Lamp", 24.99m), new ProductType("Pendant Light", 44.99m),
            new ProductType("Ceiling Fan", 89.99m), new ProductType("Stainless Steel Cookware Set", 189.99m),
            new ProductType("Non-Stick Frying Pan", 24.99m), new ProductType("Cast Iron Skillet", 39.99m),
            new ProductType("Stand Mixer", 249.99m), new ProductType("Hand Mixer", 29.99m),
            new ProductType("Blender", 59.99m), new ProductType("Food Processor", 89.99m),
            new ProductType("Electric Kettle", 34.99m), new ProductType("Espresso Machine", 179.99m),
            new ProductType("Drip Coffee Maker", 49.99m), new ProductType("Air Fryer", 89.99m),
            new ProductType("Toaster", 24.99m), new ProductType("Toaster Oven", 69.99m),
            new ProductType("Slow Cooker", 44.99m), new ProductType("Pressure Cooker", 79.99m),
            new ProductType("Knife Set", 79.99m), new ProductType("Cutting Board", 17.99m),
            new ProductType("Dinnerware Set", 69.99m), new ProductType("Glassware Set", 34.99m),
            new ProductType("Cutlery Set", 29.99m), new ProductType("Storage Container Set", 24.99m),
            new ProductType("Bath Towel Set", 39.99m), new ProductType("Bedding Duvet Set", 59.99m),
            new ProductType("Pillow", 19.99m), new ProductType("Mattress Topper", 89.99m),
            new ProductType("Blackout Curtains", 44.99m), new ProductType("Area Rug", 79.99m),
            new ProductType("Throw Blanket", 27.99m), new ProductType("Wall Clock", 22.99m),
            new ProductType("Picture Frame", 12.99m), new ProductType("Scented Candle", 16.99m),
            new ProductType("Diffuser", 29.99m), new ProductType("Vacuum Cleaner", 199.99m),
            new ProductType("Steam Mop", 69.99m), new ProductType("Laundry Hamper", 24.99m)
        }),

        new("Clothing", new[] { "Vertex", "Aurora" }, ColorVariants, new[]
        {
            new ProductType("Men's T-Shirt", 19.99m), new ProductType("Women's T-Shirt", 19.99m),
            new ProductType("Men's Jeans", 54.99m), new ProductType("Women's Jeans", 54.99m),
            new ProductType("Men's Chinos", 44.99m), new ProductType("Women's Leggings", 34.99m),
            new ProductType("Men's Shorts", 24.99m), new ProductType("Women's Shorts", 24.99m),
            new ProductType("Men's Hoodie", 44.99m), new ProductType("Women's Hoodie", 44.99m),
            new ProductType("Men's Jacket", 89.99m), new ProductType("Women's Jacket", 89.99m),
            new ProductType("Men's Jumper", 49.99m), new ProductType("Women's Cardigan", 49.99m),
            new ProductType("Men's Polo Shirt", 27.99m), new ProductType("Women's Blouse", 34.99m),
            new ProductType("Men's Dress Shirt", 39.99m), new ProductType("Women's Dress", 59.99m),
            new ProductType("Men's Suit", 199.99m), new ProductType("Women's Skirt", 34.99m),
            new ProductType("Men's Running Shoes", 99.99m), new ProductType("Women's Running Shoes", 99.99m),
            new ProductType("Men's Casual Sneakers", 69.99m), new ProductType("Women's Casual Sneakers", 69.99m),
            new ProductType("Men's Formal Shoes", 89.99m), new ProductType("Women's Ankle Boots", 79.99m),
            new ProductType("Men's Sandals", 29.99m), new ProductType("Women's Sandals", 29.99m),
            new ProductType("Men's Belt", 22.99m), new ProductType("Women's Handbag", 64.99m),
            new ProductType("Men's Wallet", 24.99m), new ProductType("Women's Sunglasses", 39.99m),
            new ProductType("Men's Cap", 17.99m), new ProductType("Women's Scarf", 32.99m),
            new ProductType("Unisex Beanie", 14.99m), new ProductType("Unisex Gloves", 16.99m),
            new ProductType("Men's Socks (5-Pack)", 12.99m), new ProductType("Women's Socks (5-Pack)", 12.99m),
            new ProductType("Men's Swim Shorts", 24.99m), new ProductType("Women's Swimsuit", 39.99m)
        }),

        new("Sports & Outdoors", new[] { "Nimbus", "Cascade" }, ColorVariants, new[]
        {
            new ProductType("Yoga Mat", 24.99m), new ProductType("Foam Roller", 22.99m),
            new ProductType("Resistance Bands Set", 17.99m), new ProductType("Dumbbell Set", 149.99m),
            new ProductType("Kettlebell", 34.99m), new ProductType("Barbell", 89.99m),
            new ProductType("Weight Bench", 119.99m), new ProductType("Pull-Up Bar", 39.99m),
            new ProductType("Jump Rope", 12.99m), new ProductType("Exercise Ball", 19.99m),
            new ProductType("Camping Tent", 89.99m), new ProductType("Sleeping Bag", 64.99m),
            new ProductType("Camping Chair", 34.99m), new ProductType("Camping Stove", 49.99m),
            new ProductType("Cooler Box", 44.99m), new ProductType("Hiking Backpack", 74.99m),
            new ProductType("Trekking Poles", 29.99m), new ProductType("Water Bottle", 19.99m),
            new ProductType("Hydration Pack", 39.99m), new ProductType("Headlamp", 22.99m),
            new ProductType("Cycling Helmet", 44.99m), new ProductType("Road Bike", 599.99m),
            new ProductType("Mountain Bike", 699.99m), new ProductType("Bike Lock", 24.99m),
            new ProductType("Bike Pump", 17.99m), new ProductType("Football", 19.99m),
            new ProductType("Basketball", 24.99m), new ProductType("Tennis Racket", 59.99m),
            new ProductType("Golf Club Set", 349.99m), new ProductType("Fishing Rod", 44.99m),
            new ProductType("Swimming Goggles", 14.99m), new ProductType("Wetsuit", 129.99m),
            new ProductType("Surfboard", 349.99m), new ProductType("Skateboard", 69.99m),
            new ProductType("Snowboard", 299.99m), new ProductType("Ski Goggles", 49.99m),
            new ProductType("Fitness Tracker Watch", 79.99m), new ProductType("Smart Scale", 34.99m),
            new ProductType("Gym Bag", 27.99m), new ProductType("First Aid Kit", 19.99m)
        }),

        new("Books", new[] { "Acme" }, BookFormats, new[]
        {
            new ProductType("The Pragmatic Programmer", 34.99m), new ProductType("Clean Code", 32.99m),
            new ProductType("Atomic Habits", 14.99m), new ProductType("The Midnight Library", 9.99m),
            new ProductType("Sapiens", 12.99m), new ProductType("Project Hail Mary", 10.99m),
            new ProductType("The Silent Patient", 8.99m), new ProductType("Educated", 11.99m),
            new ProductType("Deep Work", 13.99m), new ProductType("The Design of Everyday Things", 22.99m),
            new ProductType("Thinking, Fast and Slow", 13.99m), new ProductType("Where the Crawdads Sing", 9.99m),
            new ProductType("The Alchemist", 8.99m), new ProductType("Dune", 11.99m),
            new ProductType("Becoming", 12.99m), new ProductType("The Psychology of Money", 13.99m),
            new ProductType("A Brief History of Time", 10.99m), new ProductType("The Hobbit", 9.99m),
            new ProductType("1984", 8.99m), new ProductType("Man's Search for Meaning", 9.99m),
            new ProductType("To Kill a Mockingbird", 8.99m), new ProductType("Pride and Prejudice", 7.99m),
            new ProductType("The Great Gatsby", 7.99m), new ProductType("Brave New World", 8.99m),
            new ProductType("Fahrenheit 451", 8.99m), new ProductType("The Catcher in the Rye", 8.99m),
            new ProductType("Animal Farm", 6.99m), new ProductType("Lord of the Flies", 7.99m),
            new ProductType("The Lord of the Rings", 16.99m), new ProductType("Harry Potter and the Philosopher's Stone", 9.99m),
            new ProductType("The Da Vinci Code", 9.99m), new ProductType("Gone Girl", 9.99m),
            new ProductType("The Girl with the Dragon Tattoo", 9.99m), new ProductType("Normal People", 8.99m),
            new ProductType("Circe", 9.99m), new ProductType("The Song of Achilles", 9.99m),
            new ProductType("Klara and the Sun", 10.99m), new ProductType("The Seven Husbands of Evelyn Hugo", 9.99m),
            new ProductType("It Ends with Us", 9.99m), new ProductType("Verity", 9.99m)
        })
    };

    public static async Task SeedAsync(ECommerceDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return; // Already seeded — safe to call on every startup
        }

        DateTime now = DateTime.UtcNow;
        var random = new Random(12345); // fixed seed => reproducible demo data across environments

        string[] brandNames = Catalogue.SelectMany(c => c.Brands).Distinct().ToArray();
        Dictionary<string, Brand> brandsByName = brandNames.ToDictionary(
            name => name,
            name => new Brand
            {
                Id = Guid.NewGuid(),
                Name = name,
                Slug = Slugify(name),
                LogoUrl = null,
                IsActive = true,
                CreatedAt = now
            });

        await dbContext.Brands.AddRangeAsync(brandsByName.Values, cancellationToken);

        var products = new List<Product>(1000);

        foreach (CategorySeed categorySeed in Catalogue)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                ParentId = null,
                Name = categorySeed.Name,
                Slug = Slugify(categorySeed.Name),
                Description = $"{categorySeed.Name} category",
                ImageUrl = null,
                IsActive = true,
                CreatedAt = now
            };
            await dbContext.Categories.AddAsync(category, cancellationToken);

            Brand[] categoryBrands = categorySeed.Brands.Select(b => brandsByName[b]).ToArray();

            foreach (ProductType type in categorySeed.Types)
            {
                foreach (string variant in categorySeed.Variants)
                {
                    Brand brand = categoryBrands[random.Next(categoryBrands.Length)];
                    string name = $"{type.BaseName} - {variant}";

                    // +/-5% price jitter per variant so identical variants aren't all exactly the same price
                    decimal jitter = 1 + ((decimal)(random.NextDouble() * 0.1) - 0.05m);

                    var price = new Money(Math.Round(type.Price * jitter, 2), CurrencyConstants.CurrencyGBPound);
                    int stockQuantity = random.Next(0, 500); // Random stock quantity between 0 and 99

                    var product = new Product
                    {
                        Id = Guid.NewGuid(),
                        CategoryId = category.Id,
                        BrandId = brand.Id,
                        Name = name,
                        Slug = Slugify(name),
                        Description = $"{type.BaseName} in {variant} from {brand.Name}.",
                        ShortDescription = name,
                        BasePrice = price,
                        IsActive = true,
                        IsFeatured = random.Next(0, 20) == 0, // ~5% featured
                        CreatedAt = now
                    };

                    product.IncreaseStock(stockQuantity);
                    products.Add(product);
                }
            }
        }

        await dbContext.Products.AddRangeAsync(products, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string Slugify(string value) =>
        value.ToLowerInvariant()
             .Replace(" & ", "-")
             .Replace("'", "")
             .Replace(",", "")
             .Replace(":", "")
             .Replace(" ", "-");
}
