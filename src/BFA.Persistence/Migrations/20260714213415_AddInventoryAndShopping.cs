using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryAndShopping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shopping");

            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "shopping_carts",
                schema: "shopping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stock_items",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OnHand = table.Column<int>(type: "integer", nullable: false),
                    Reserved = table.Column<int>(type: "integer", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shopping_cart_items",
                schema: "shopping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShoppingCartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_cart_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shopping_cart_items_shopping_carts_ShoppingCartId",
                        column: x => x.ShoppingCartId,
                        principalSchema: "shopping",
                        principalTable: "shopping_carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "wishlist_items",
                schema: "shopping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShoppingCartId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wishlist_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wishlist_items_shopping_carts_ShoppingCartId",
                        column: x => x.ShoppingCartId,
                        principalSchema: "shopping",
                        principalTable: "shopping_carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_reservations_stock_items_StockItemId",
                        column: x => x.StockItemId,
                        principalSchema: "inventory",
                        principalTable: "stock_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shopping_cart_items_ShoppingCartId_ProductVariantId",
                schema: "shopping",
                table: "shopping_cart_items",
                columns: new[] { "ShoppingCartId", "ProductVariantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_items_ProductVariantId",
                schema: "inventory",
                table: "stock_items",
                column: "ProductVariantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_items_SupplierId",
                schema: "inventory",
                table: "stock_items",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_ReferenceId",
                schema: "inventory",
                table: "stock_reservations",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_stock_reservations_StockItemId_Status",
                schema: "inventory",
                table: "stock_reservations",
                columns: new[] { "StockItemId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_wishlist_items_ShoppingCartId_ProductId",
                schema: "shopping",
                table: "wishlist_items",
                columns: new[] { "ShoppingCartId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shopping_cart_items",
                schema: "shopping");

            migrationBuilder.DropTable(
                name: "stock_reservations",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "wishlist_items",
                schema: "shopping");

            migrationBuilder.DropTable(
                name: "stock_items",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "shopping_carts",
                schema: "shopping");
        }
    }
}
