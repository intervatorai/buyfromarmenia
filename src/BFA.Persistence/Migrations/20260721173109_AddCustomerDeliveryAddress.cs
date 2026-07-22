using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDeliveryAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
