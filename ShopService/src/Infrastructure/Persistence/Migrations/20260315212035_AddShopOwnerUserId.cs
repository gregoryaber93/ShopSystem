using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopOwnerUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerUserId",
                table: "shops",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_shops_OwnerUserId",
                table: "shops",
                column: "OwnerUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_shops_OwnerUserId",
                table: "shops");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "shops");
        }
    }
}
