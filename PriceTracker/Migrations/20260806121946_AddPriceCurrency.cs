using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "PriceHistories",
                newName: "PriceAmount");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAmount",
                table: "PriceHistories",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrencyCode",
                table: "PriceHistories",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PLN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceCurrencyCode",
                table: "PriceHistories");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAmount",
                table: "PriceHistories",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.RenameColumn(
                name: "PriceAmount",
                table: "PriceHistories",
                newName: "Price");
        }
    }
}
