using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Carts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Categories_ECOMMERCE_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ECOMMERCE_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShippingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OneTimePasswordSecret = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsTwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShortDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Products_ECOMMERCE_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "ECOMMERCE_Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Products_ECOMMERCE_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ECOMMERCE_Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_OrderStatusHistory_ECOMMERCE_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ECOMMERCE_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Payments_ECOMMERCE_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ECOMMERCE_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippingMethod = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ShippedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Shipments_ECOMMERCE_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ECOMMERCE_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_PendingTwoFactorLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PendingTwoFactorToken = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PendingTokenExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_PendingTwoFactorLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_PendingTwoFactorLogins_ECOMMERCE_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "ECOMMERCE_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_RefreshTokens_ECOMMERCE_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "ECOMMERCE_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_UserAddresss",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    County = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    UserId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_UserAddresss", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_UserAddresss_ECOMMERCE_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "ECOMMERCE_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_UserAddresss_ECOMMERCE_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "ECOMMERCE_Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_UserRoles_ECOMMERCE_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ECOMMERCE_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_UserRoles_ECOMMERCE_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "ECOMMERCE_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_CartItems_ECOMMERCE_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "ECOMMERCE_Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_CartItems_ECOMMERCE_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ECOMMERCE_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPriceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_OrderItems_ECOMMERCE_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "ECOMMERCE_Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_OrderItems_ECOMMERCE_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ECOMMERCE_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_ProductImages_ECOMMERCE_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ECOMMERCE_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ECOMMERCE_Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ECOMMERCE_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Reviews_ECOMMERCE_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ECOMMERCE_Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ECOMMERCE_Reviews_ECOMMERCE_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "ECOMMERCE_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_CartItems_CartId",
                table: "ECOMMERCE_CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_CartItems_ProductId",
                table: "ECOMMERCE_CartItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Carts_UserId",
                table: "ECOMMERCE_Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Categories_ParentId",
                table: "ECOMMERCE_Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_OrderItems_OrderId",
                table: "ECOMMERCE_OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_OrderItems_ProductId",
                table: "ECOMMERCE_OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Orders_OrderNumber",
                table: "ECOMMERCE_Orders",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_OrderStatusHistory_OrderId",
                table: "ECOMMERCE_OrderStatusHistory",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Payments_OrderId",
                table: "ECOMMERCE_Payments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Payments_TransactionId",
                table: "ECOMMERCE_Payments",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_PendingTwoFactorLogins_PendingTwoFactorToken",
                table: "ECOMMERCE_PendingTwoFactorLogins",
                column: "PendingTwoFactorToken");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_PendingTwoFactorLogins_UserId",
                table: "ECOMMERCE_PendingTwoFactorLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_ProductImages_ProductId",
                table: "ECOMMERCE_ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Products_BrandId",
                table: "ECOMMERCE_Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Products_CategoryId",
                table: "ECOMMERCE_Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_RefreshTokens_Token",
                table: "ECOMMERCE_RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_RefreshTokens_UserId",
                table: "ECOMMERCE_RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Reviews_ProductId",
                table: "ECOMMERCE_Reviews",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Reviews_UserId",
                table: "ECOMMERCE_Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_Shipments_OrderId",
                table: "ECOMMERCE_Shipments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_UserAddresss_UserId",
                table: "ECOMMERCE_UserAddresss",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_UserAddresss_UserId1",
                table: "ECOMMERCE_UserAddresss",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_ECOMMERCE_UserRoles_RoleId",
                table: "ECOMMERCE_UserRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ECOMMERCE_CartItems");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_OrderItems");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Payments");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_PendingTwoFactorLogins");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_ProductImages");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_RefreshTokens");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Reviews");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Shipments");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_UserAddresss");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_UserRoles");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Carts");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Products");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Orders");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Roles");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Users");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Brands");

            migrationBuilder.DropTable(
                name: "ECOMMERCE_Categories");
        }
    }
}
