using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderingAndFulfillment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.EnsureSchema(
                name: "fulfillment");

            migrationBuilder.CreateTable(
                name: "customer_orders",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CartId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    CustomerFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shipping_country_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    shipping_city = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    shipping_line1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    shipping_line2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    shipping_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    shipping_region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    PaymentStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    FulfillmentStatus = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_orders",
                schema: "fulfillment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer_order_items",
                schema: "ordering",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SupplierSku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_order_items_customer_orders_CustomerOrderId",
                        column: x => x.CustomerOrderId,
                        principalSchema: "ordering",
                        principalTable: "customer_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_order_items",
                schema: "fulfillment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SupplierSku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_order_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_order_items_supplier_orders_SupplierOrderId",
                        column: x => x.SupplierOrderId,
                        principalSchema: "fulfillment",
                        principalTable: "supplier_orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_order_items_CustomerOrderId",
                schema: "ordering",
                table: "customer_order_items",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_orders_CartId",
                schema: "ordering",
                table: "customer_orders",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_orders_OrderNumber",
                schema: "ordering",
                table: "customer_orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_order_items_SupplierOrderId",
                schema: "fulfillment",
                table: "supplier_order_items",
                column: "SupplierOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_orders_CustomerOrderId",
                schema: "fulfillment",
                table: "supplier_orders",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_orders_SupplierId",
                schema: "fulfillment",
                table: "supplier_orders",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_order_items",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "supplier_order_items",
                schema: "fulfillment");

            migrationBuilder.DropTable(
                name: "customer_orders",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "supplier_orders",
                schema: "fulfillment");
        }
    }
}
