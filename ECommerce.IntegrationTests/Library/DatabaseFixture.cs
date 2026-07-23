using ECommerce.Domain.Entities.User;
using ECommerce.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Xunit;

namespace ECommerce.IntegrationTests.Library;

public sealed class SqlServerFixture : IAsyncLifetime
{  
    private readonly MsSqlContainer _container =
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();

    public string ConnectionString { get; private set; } = null!;
    private readonly Guid[] _seededUserIds =
    [
        Guid.Parse("11111111111111111111111111111112"),
        Guid.Parse("22222222222222222222222222222223")
    ];

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var builder = new SqlConnectionStringBuilder(
            _container.GetConnectionString())
        {
            InitialCatalog = "DevHabit"
        };

        ConnectionString = builder.ConnectionString;

        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var db = new ECommerceDbContext(options);

        // Creates database and applies migrations
        await db.Database.MigrateAsync();

        // Debug: verify expected tables exist in the test database and print listing if they do not.
        // This helps diagnose "Cannot find the object \"ECOMMERCE_Users\"" errors when running tests.
        await using (var verifyConn = new SqlConnection(ConnectionString))
        {
            await verifyConn.OpenAsync();

            using SqlCommand verifyCmd = verifyConn.CreateCommand();
            verifyCmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ECOMMERCE_Users'";
            object? result = await verifyCmd.ExecuteScalarAsync();
            int count = Convert.ToInt32(result ?? 0);

            if (count == 0)
            {
                // dump existing tables for debugging
                using SqlCommand listCmd = verifyConn.CreateCommand();
                listCmd.CommandText = "SELECT TABLE_SCHEMA + '.' + TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_SCHEMA, TABLE_NAME";
                using SqlDataReader reader = await listCmd.ExecuteReaderAsync();

                Console.WriteLine("---- Database tables in test DB ----");
                while (await reader.ReadAsync())
                {
                    Console.WriteLine(reader.GetString(0));
                }
                Console.WriteLine("------------------------------------");

                throw new InvalidOperationException("ECOMMERCE_Users table not found after migrations. Ensure migrations applied to the same database used by the test host.");
            }
        }

        // Insert seeded users only if missing (handles re-runs / existing DB)
        if (!await db.Users.AnyAsync(u => u.Id == _seededUserIds[0] || u.Id == _seededUserIds[1]))
        {
            if (!await db.Users.AnyAsync(u => u.Email == "alice@example.com"))
            {
                await db.Users.AddAsync(new User
                {
                    Id = _seededUserIds[0], 
                    Email = "alice@example.com",
                    PasswordHash = "$2a$11$K56YkkSU4tfH8B9JLLyqYevpRoBo./Cd3IkYQp2WZjmeMyTqCeKVy",
                    Status = "Active",
                    LastName = "Smith",
                    FirstName = "Alice",
                    Phone = "1234567890",
                });
            }

            if (!await db.Users.AnyAsync(u => u.Email == "john@example.com"))
            {
                await db.Users.AddAsync(new User
                {
                    Id = _seededUserIds[1], 
                    Email = "john@example.com",
                    PasswordHash = "$2a$11$K56YkkSU4tfH8B9JLLyqYevpRoBo./Cd3IkYQp2WZjmeMyTqCeKVy",
                    Status = "Active",
                    LastName = "Smith",
                    FirstName = "Alice",
                    Phone = "1234567890",
                });
            }

            await db.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Clear non-seeded data so tests can run without recreating the database.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        DbContextOptions<ECommerceDbContext> options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        await using var db = new ECommerceDbContext(options);

        IQueryable<User> usersToRemove = db.Users.Where(u => !_seededUserIds.Contains(u.Id));
        db.Users.RemoveRange(usersToRemove);
        await db.SaveChangesAsync();
        // Ensure seeded users still exist (in case upstream cleanup removed them)
        foreach (Guid id in _seededUserIds)
        {
            if (!await db.Users.AnyAsync(u => u.Id == id))
            {
                // re-add minimal seeded user data for missing seeded id
                string username = id == _seededUserIds[0] ? "alice" : "john";
                string email = id == _seededUserIds[0] ? "alice@example.com" : "john@example.com";

                await db.Users.AddAsync(new User
                {
                    Id = id,
                    Email = email,
                    PasswordHash = "$2a$11$K56YkkSU4tfH8B9JLLyqYevpRoBo./Cd3IkYQp2WZjmeMyTqCeKVy",
                    Status = "Active",
                    LastName = "Smith",
                    FirstName = "Alice",
                    Phone = "1234567890",
                });

                await db.SaveChangesAsync();
            }
        }
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
