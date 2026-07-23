using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingRatesAndOrderFees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedWeightKg",
                schema: "ordering",
                table: "customer_orders",
                type: "numeric(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAdjustmentReason",
                schema: "ordering",
                table: "customer_orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFee",
                schema: "ordering",
                table: "customer_orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeQuoted",
                schema: "ordering",
                table: "customer_orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingMarginPercent",
                schema: "ordering",
                table: "customer_orders",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "shipping_pricing_settings",
                schema: "shipping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ErrorMarginPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_pricing_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipping_rate_brackets",
                schema: "shipping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CountryIsoCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    WeightFromKg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    WeightToKg = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_rate_brackets", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "shipping",
                table: "shipping_pricing_settings",
                columns: new[] { "Id", "ErrorMarginPercent", "UpdatedAtUtc" },
                values: new object[] { new Guid("b2c3d4e5-f6a7-8901-bcde-f12345678901"), 10m, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                schema: "shipping",
                table: "shipping_rate_brackets",
                columns: new[] { "Id", "CountryIsoCode", "WeightFromKg", "WeightToKg", "Price", "Currency", "IsActive", "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("c1000001-0000-4000-8000-000000000001"), "AM", 0m, 1m, 5m, "USD", true, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c1000001-0000-4000-8000-000000000002"), "AM", 1.001m, 5m, 12m, "USD", true, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c1000001-0000-4000-8000-000000000003"), "US", 0m, 1m, 18m, "USD", true, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c1000001-0000-4000-8000-000000000004"), "US", 1.001m, 5m, 35m, "USD", true, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c1000001-0000-4000-8000-000000000005"), "US", 5.001m, 20m, 65m, "USD", true, new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 22, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_shipping_rate_brackets_CountryIsoCode",
                schema: "shipping",
                table: "shipping_rate_brackets",
                column: "CountryIsoCode");

            migrationBuilder.CreateIndex(
                name: "IX_shipping_rate_brackets_CountryIsoCode_IsActive",
                schema: "shipping",
                table: "shipping_rate_brackets",
                columns: new[] { "CountryIsoCode", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipping_pricing_settings",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "shipping_rate_brackets",
                schema: "shipping");

            migrationBuilder.DropColumn(
                name: "EstimatedWeightKg",
                schema: "ordering",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "ShippingAdjustmentReason",
                schema: "ordering",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "ShippingFee",
                schema: "ordering",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "ShippingFeeQuoted",
                schema: "ordering",
                table: "customer_orders");

            migrationBuilder.DropColumn(
                name: "ShippingMarginPercent",
                schema: "ordering",
                table: "customer_orders");
        }
    }
}
