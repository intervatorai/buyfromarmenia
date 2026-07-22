using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDeliveryAddressesEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryCity",
                schema: "identity",
                table: "customer_profiles");

            migrationBuilder.DropColumn(
                name: "DeliveryCountryCode",
                schema: "identity",
                table: "customer_profiles");

            migrationBuilder.DropColumn(
                name: "DeliveryLine1",
                schema: "identity",
                table: "customer_profiles");

            migrationBuilder.DropColumn(
                name: "DeliveryLine2",
                schema: "identity",
                table: "customer_profiles");

            migrationBuilder.DropColumn(
                name: "DeliveryPostalCode",
                schema: "identity",
                table: "customer_profiles");

            migrationBuilder.DropColumn(
                name: "DeliveryRegion",
                schema: "identity",
                table: "customer_profiles");

            migrationBuilder.CreateTable(
                name: "customer_delivery_addresses",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Line1 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Line2 = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Region = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_delivery_addresses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customer_delivery_addresses_UserId",
                schema: "identity",
                table: "customer_delivery_addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_delivery_addresses_UserId_IsDefault",
                schema: "identity",
                table: "customer_delivery_addresses",
                columns: new[] { "UserId", "IsDefault" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customer_delivery_addresses",
                schema: "identity");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCity",
                schema: "identity",
                table: "customer_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryCountryCode",
                schema: "identity",
                table: "customer_profiles",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryLine1",
                schema: "identity",
                table: "customer_profiles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryLine2",
                schema: "identity",
                table: "customer_profiles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryPostalCode",
                schema: "identity",
                table: "customer_profiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryRegion",
                schema: "identity",
                table: "customer_profiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }
    }
}
