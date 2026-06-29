using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PriceTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistory_TrackedProducts_TrackedProductId",
                table: "PriceHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceHistory",
                table: "PriceHistory");

            migrationBuilder.RenameTable(
                name: "PriceHistory",
                newName: "PriceHistories");

            migrationBuilder.RenameIndex(
                name: "IX_PriceHistory_TrackedProductId",
                table: "PriceHistories",
                newName: "IX_PriceHistories_TrackedProductId");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "TrackedProducts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceHistories",
                table: "PriceHistories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackedProducts_UserId",
                table: "TrackedProducts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistories_TrackedProducts_TrackedProductId",
                table: "PriceHistories",
                column: "TrackedProductId",
                principalTable: "TrackedProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrackedProducts_Users_UserId",
                table: "TrackedProducts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PriceHistories_TrackedProducts_TrackedProductId",
                table: "PriceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TrackedProducts_Users_UserId",
                table: "TrackedProducts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TrackedProducts_UserId",
                table: "TrackedProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriceHistories",
                table: "PriceHistories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "TrackedProducts");

            migrationBuilder.RenameTable(
                name: "PriceHistories",
                newName: "PriceHistory");

            migrationBuilder.RenameIndex(
                name: "IX_PriceHistories_TrackedProductId",
                table: "PriceHistory",
                newName: "IX_PriceHistory_TrackedProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriceHistory",
                table: "PriceHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PriceHistory_TrackedProducts_TrackedProductId",
                table: "PriceHistory",
                column: "TrackedProductId",
                principalTable: "TrackedProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
