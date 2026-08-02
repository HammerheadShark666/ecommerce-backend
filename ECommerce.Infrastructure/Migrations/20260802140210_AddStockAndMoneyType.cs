using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStockAndMoneyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BasePrice",
                table: "ECOMMERCE_Products",
                newName: "Price");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ECOMMERCE_Products",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "ECOMMERCE_Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ECOMMERCE_Products");

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "ECOMMERCE_Products");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "ECOMMERCE_Products",
                newName: "BasePrice");
        }
    }
}
