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

            // Use raw SQL instead of InsertData: this migration's Designer is a stub
            // (no target model), and Npgsql cannot generate InsertData without column types.
            migrationBuilder.Sql("""
                INSERT INTO shipping.shipping_pricing_settings ("Id", "ErrorMarginPercent", "UpdatedAtUtc")
                VALUES ('b2c3d4e5-f6a7-8901-bcde-f12345678901', 10, TIMESTAMPTZ '2026-07-22 00:00:00+00')
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO shipping.shipping_rate_brackets
                    ("Id", "CountryIsoCode", "WeightFromKg", "WeightToKg", "Price", "Currency", "IsActive", "CreatedAtUtc", "UpdatedAtUtc")
                VALUES
                    ('c1000001-0000-4000-8000-000000000001', 'AM', 0, 1, 5, 'USD', TRUE, TIMESTAMPTZ '2026-07-22 00:00:00+00', TIMESTAMPTZ '2026-07-22 00:00:00+00'),
                    ('c1000001-0000-4000-8000-000000000002', 'AM', 1.001, 5, 12, 'USD', TRUE, TIMESTAMPTZ '2026-07-22 00:00:00+00', TIMESTAMPTZ '2026-07-22 00:00:00+00'),
                    ('c1000001-0000-4000-8000-000000000003', 'US', 0, 1, 18, 'USD', TRUE, TIMESTAMPTZ '2026-07-22 00:00:00+00', TIMESTAMPTZ '2026-07-22 00:00:00+00'),
                    ('c1000001-0000-4000-8000-000000000004', 'US', 1.001, 5, 35, 'USD', TRUE, TIMESTAMPTZ '2026-07-22 00:00:00+00', TIMESTAMPTZ '2026-07-22 00:00:00+00'),
                    ('c1000001-0000-4000-8000-000000000005', 'US', 5.001, 20, 65, 'USD', TRUE, TIMESTAMPTZ '2026-07-22 00:00:00+00', TIMESTAMPTZ '2026-07-22 00:00:00+00')
                ON CONFLICT ("Id") DO NOTHING;
                """);
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
