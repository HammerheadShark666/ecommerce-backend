using ECommerce.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Seed_Data;

public static class DatabaseSeeder
{
    private const string SeedUser = "seed-script";

    private sealed record CategorySeed(string Name, string[] Brands, (string Name, decimal Price)[] Products);

    private static readonly CategorySeed[] Catalogue =
    {
        new("Electronics",
            new[] { "Acme", "Nimbus", "Vertex" },
            new (string, decimal)[]
            {
                ("Wireless Noise-Cancelling Headphones", 199.99m),
                ("Bluetooth Portable Speaker", 59.99m),
                ("27-inch 4K Monitor", 329.99m),
                ("Mechanical Gaming Keyboard", 89.99m),
                ("Wireless Ergonomic Mouse", 34.99m),
                ("USB-C Docking Station", 129.99m),
                ("Smart Fitness Watch", 149.99m),
                ("Portable SSD 1TB", 109.99m),
                ("Webcam 1080p", 44.99m),
                ("Wireless Charging Pad", 24.99m),
                ("Action Camera 4K", 249.99m),
                ("Home Theatre Soundbar", 179.99m),
                ("Smart Home Hub", 79.99m),
                ("Robot Vacuum Cleaner", 299.99m),
                ("Wireless Earbuds", 69.99m),
                ("Streaming Media Player", 39.99m),
                ("Power Bank 20000mAh", 29.99m),
                ("Smart Plug (4-Pack)", 19.99m),
                ("Laptop Stand Adjustable", 27.99m),
                ("Wireless Router Wi-Fi 6", 149.99m)
            }),

        new("Home & Kitchen",
            new[] { "Cascade", "Aurora" },
            new (string, decimal)[]
            {
                ("Stainless Steel Cookware Set", 189.99m),
                ("Non-Stick Frying Pan 28cm", 24.99m),
                ("Stand Mixer 5.5L", 249.99m),
                ("Electric Kettle 1.7L", 34.99m),
                ("Espresso Coffee Machine", 179.99m),
                ("Air Fryer 5.5L", 89.99m),
                ("Knife Block Set", 79.99m),
                ("Ceramic Dinnerware Set (16pc)", 69.99m),
                ("Memory Foam Pillow", 29.99m),
                ("Cotton Bath Towel Set", 39.99m),
                ("Blackout Curtains (Pair)", 44.99m),
                ("Scented Candle Gift Set", 19.99m),
                ("Vacuum Cleaner Cordless", 199.99m),
                ("Food Storage Container Set", 24.99m),
                ("Bamboo Cutting Board", 17.99m),
                ("Electric Toothbrush", 49.99m),
                ("Digital Kitchen Scale", 14.99m),
                ("Bedding Duvet Set Queen", 59.99m),
                ("Table Lamp Modern", 34.99m),
                ("Throw Blanket Knitted", 27.99m)
            }),

        new("Clothing",
            new[] { "Vertex", "Aurora" },
            new (string, decimal)[]
            {
                ("Men's Slim Fit Chinos", 44.99m),
                ("Women's High-Waist Jeans", 54.99m),
                ("Unisex Cotton T-Shirt (3-Pack)", 29.99m),
                ("Men's Merino Wool Jumper", 69.99m),
                ("Women's Puffer Jacket", 89.99m),
                ("Men's Oxford Shirt", 39.99m),
                ("Women's Midi Dress", 59.99m),
                ("Men's Running Shorts", 24.99m),
                ("Women's Yoga Leggings", 34.99m),
                ("Unisex Hoodie", 44.99m),
                ("Men's Leather Belt", 22.99m),
                ("Women's Cashmere Scarf", 32.99m),
                ("Men's Chino Shorts", 29.99m),
                ("Women's Blazer Fitted", 74.99m),
                ("Unisex Beanie Hat", 14.99m),
                ("Men's Denim Jacket", 64.99m),
                ("Women's Wrap Cardigan", 49.99m),
                ("Men's Polo Shirt", 27.99m),
                ("Women's Ankle Boots", 79.99m),
                ("Unisex Wool Socks (5-Pack)", 19.99m)
            }),

        new("Sports & Outdoors",
            new[] { "Nimbus", "Cascade" },
            new (string, decimal)[]
            {
                ("Yoga Mat Non-Slip", 24.99m),
                ("Adjustable Dumbbell Set", 149.99m),
                ("Camping Tent 2-Person", 89.99m),
                ("Insulated Water Bottle 1L", 19.99m),
                ("Resistance Bands Set", 17.99m),
                ("Hiking Backpack 40L", 74.99m),
                ("Foam Roller", 22.99m),
                ("Running Shoes Trail", 99.99m),
                ("Sleeping Bag 3-Season", 64.99m),
                ("Cycling Helmet", 44.99m),
                ("Jump Rope Speed", 12.99m),
                ("Camping Chair Folding", 34.99m),
                ("Football Size 5", 19.99m),
                ("Basketball Indoor/Outdoor", 24.99m),
                ("Fitness Tracker Band", 39.99m),
                ("Trekking Poles (Pair)", 29.99m),
                ("Gym Duffel Bag", 27.99m),
                ("Swimming Goggles", 14.99m),
                ("Portable Camping Stove", 49.99m),
                ("Exercise Ball 65cm", 19.99m)
            }),

        new("Books",
            new[] { "Acme" },
            new (string, decimal)[]
            {
                ("The Pragmatic Programmer", 34.99m),
                ("Clean Code", 32.99m),
                ("Atomic Habits", 14.99m),
                ("The Midnight Library", 9.99m),
                ("Sapiens: A Brief History of Humankind", 12.99m),
                ("Project Hail Mary", 10.99m),
                ("The Silent Patient", 8.99m),
                ("Educated: A Memoir", 11.99m),
                ("Deep Work", 13.99m),
                ("The Design of Everyday Things", 22.99m),
                ("Thinking, Fast and Slow", 13.99m),
                ("Where the Crawdads Sing", 9.99m),
                ("The Alchemist", 8.99m),
                ("Dune", 11.99m),
                ("Becoming", 12.99m),
                ("The Psychology of Money", 13.99m),
                ("A Brief History of Time", 10.99m),
                ("The Hobbit", 9.99m),
                ("1984", 8.99m),
                ("Man's Search for Meaning", 9.99m)
            })
    };

    public static async Task SeedAsync(ECommerceDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Products.AnyAsync(cancellationToken))
        {
            return; // Already seeded — safe to call on every startup
        }

        DateTime now = DateTime.UtcNow;

        // Distinct brand names across the whole catalogue
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

        var categories = Catalogue.Select(c => new Category
        {
            Id = Guid.NewGuid(),
            ParentId = null,
            Name = c.Name,
            Slug = Slugify(c.Name),
            Description = $"{c.Name} category",
            ImageUrl = null,
            IsActive = true,
            CreatedAt = now
        }).ToList();

        await dbContext.Brands.AddRangeAsync(brandsByName.Values, cancellationToken);
        await dbContext.Categories.AddRangeAsync(categories, cancellationToken);

        var random = new Random(12345); // fixed seed => reproducible demo data across environments
        var products = new List<Product>();

        for (int i = 0; i < Catalogue.Length; i++)
        {
            CategorySeed categorySeed = Catalogue[i];
            Category category = categories[i];
            Brand[] categoryBrands = categorySeed.Brands.Select(b => brandsByName[b]).ToArray();

            foreach ((string name, decimal price) in categorySeed.Products)
            {
                Brand brand = categoryBrands[random.Next(categoryBrands.Length)];

                products.Add(new Product
                {
                    Id = Guid.NewGuid(),
                    CategoryId = category.Id,
                    BrandId = brand.Id,
                    Name = name,
                    Slug = Slugify(name),
                    Description = $"{name} — high quality {categorySeed.Name.ToLowerInvariant()} product from {brand.Name}.",
                    ShortDescription = name,
                    BasePrice = price,
                    IsActive = true,
                    IsFeatured = random.Next(0, 10) == 0, // ~10% featured
                    CreatedAt = now
                });
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
