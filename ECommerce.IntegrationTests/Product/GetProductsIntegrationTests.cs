using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Constants;
using ECommerce.Application.Features.Product.GetProducts;
using ECommerce.Domain.Entities.Product;
using ECommerce.Infrastructure.Persistence;
using ECommerce.IntegrationTests.Library;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.IntegrationTests.Product;

[Collection("Database")]
public class GetProductsIntegrationTests(SqlServerFixture fixture) : IAsyncLifetime
{
    private readonly SqlServerFixture _fixture = fixture;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private DbContextOptions<ECommerceDbContext> DbOptions => new DbContextOptionsBuilder<ECommerceDbContext>()
        .UseSqlServer(_fixture.ConnectionString)
        .Options;

    [Fact]
    public async Task GetProducts_WhenProductsExist_ReturnsOkWithPagedResults()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var category = CreateCategory();
            var brand = CreateBrand();

            db.Categories.Add(category);
            db.Brands.Add(brand);
            db.Products.Add(CreateProduct(category.Id, brand.Id, "Wireless Headphones", 199.99m));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products");

        // Assert
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result.Should().NotBeNull();
        result!.Items.Should().ContainSingle(p => p.Name == "Wireless Headphones");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetProducts_WhenNoProductsExist_ReturnsOkWithEmptyList()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        // Act
        var resp = await client.GetAsync("/products");

        // Assert
        resp.EnsureSuccessStatusCode();

        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetProducts_WhenFilteredByCategory_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var electronics = CreateCategory("Electronics");
            var books = CreateCategory("Books");
            var brand = CreateBrand();

            db.Categories.AddRange(electronics, books);
            db.Brands.Add(brand);
            db.Products.AddRange(
                CreateProduct(electronics.Id, brand.Id, "Headphones"),
                CreateProduct(books.Id, brand.Id, "Clean Code"));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products?category=Electronics");

        // Assert
        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result!.Items.Should().ContainSingle();
        result.Items.Single().Name.Should().Be("Headphones");
    }

    [Fact]
    public async Task GetProducts_WhenFilteredByPriceRange_ReturnsOnlyProductsWithinRange()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var category = CreateCategory();
            var brand = CreateBrand();

            db.Categories.Add(category);
            db.Brands.Add(brand);
            db.Products.AddRange(
                CreateProduct(category.Id, brand.Id, "Cheap Item", 9.99m),
                CreateProduct(category.Id, brand.Id, "Mid Item", 49.99m),
                CreateProduct(category.Id, brand.Id, "Expensive Item", 299.99m));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products?minPrice=20&maxPrice=100");

        // Assert
        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result!.Items.Should().ContainSingle();
        result.Items.Single().Name.Should().Be("Mid Item");
    }

    [Fact]
    public async Task GetProducts_WhenSearchTermProvided_ReturnsMatchingProducts()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var category = CreateCategory();
            var brand = CreateBrand();

            db.Categories.Add(category);
            db.Brands.Add(brand);
            db.Products.AddRange(
                CreateProduct(category.Id, brand.Id, "Wireless Headphones"),
                CreateProduct(category.Id, brand.Id, "Bluetooth Speaker"));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products?search=Headphones");

        // Assert
        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result!.Items.Should().ContainSingle();
        result.Items.Single().Name.Should().Be("Wireless Headphones");
    }

    [Fact]
    public async Task GetProducts_WhenSortedByPriceAscending_ReturnsOrderedResults()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var category = CreateCategory();
            var brand = CreateBrand();

            db.Categories.Add(category);
            db.Brands.Add(brand);
            db.Products.AddRange(
                CreateProduct(category.Id, brand.Id, "Expensive", 299.99m),
                CreateProduct(category.Id, brand.Id, "Cheap", 9.99m),
                CreateProduct(category.Id, brand.Id, "Mid", 49.99m));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products?sortBy=Price");

        // Assert
        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result!.Items.Should().BeInAscendingOrder(p => p.Price.Amount);
    }

    [Fact]
    public async Task GetProducts_WhenSecondPageRequested_SkipsFirstPageResults()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var category = CreateCategory();
            var brand = CreateBrand();

            db.Categories.Add(category);
            db.Brands.Add(brand);
            db.Products.AddRange(Enumerable.Range(1, 15)
                .Select(i => CreateProduct(category.Id, brand.Id, $"Product {i}", i)));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products?page=2&pageSize=10&sortBy=Price");

        // Assert
        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result!.Page.Should().Be(2);
        result!.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(15);
    }

    [Fact]
    public async Task GetProducts_WhenProductIsInactive_ExcludesFromResults()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        await using (var db = new ECommerceDbContext(DbOptions))
        {
            var category = CreateCategory();
            var brand = CreateBrand();

            db.Categories.Add(category);
            db.Brands.Add(brand);
            db.Products.AddRange(
                CreateProduct(category.Id, brand.Id, "Active Product"),
                CreateProduct(category.Id, brand.Id, "Discontinued Product", isActive: false));

            await db.SaveChangesAsync();
        }

        // Act
        var resp = await client.GetAsync("/products");

        // Assert
        var result = await resp.Content.ReadFromJsonAsync<GetProductsResponse>();

        result!.Items.Should().ContainSingle();
        result.Items.Single().Name.Should().Be("Active Product");
    }

    [Fact]
    public async Task GetProducts_WhenPageSizeExceeds100_ReturnsBadRequest()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        // Act
        var resp = await client.GetAsync("/products?pageSize=500");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProducts_WhenMinPriceGreaterThanMaxPrice_ReturnsBadRequest()
    {
        // Arrange
        var appFactory = new TestApplicationFactory(_fixture.ConnectionString);
        var client = appFactory.CreateClient();

        // Act
        var resp = await client.GetAsync("/products?minPrice=100&maxPrice=50");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static Category CreateCategory(string name = "Electronics") => new()
    {
        Name = name,
        Slug = name.ToLowerInvariant(),
        IsActive = true
    };

    private static Brand CreateBrand(string name = "Acme") => new()
    {
        Name = name,
        Slug = name.ToLowerInvariant(),
        IsActive = true
    };

    private static ECommerce.Domain.Entities.Product.Product CreateProduct(
        Guid categoryId,
        Guid brandId,
        string name = "Test Product",
        decimal price = 29.99m,
        string currency = CurrencyConstants.CurrencyGBPound,
        bool isActive = true) => new()
        {
            CategoryId = categoryId,
            BrandId = brandId,
            Name = name,
            Slug = name.ToLowerInvariant().Replace(" ", "-"),
            BasePrice = new Domain.ValueObjects.Money(price, currency),
            IsActive = isActive,
            IsFeatured = false
        };
}
