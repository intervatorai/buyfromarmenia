using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingCountries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shipping_countries",
                schema: "shipping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    NameHy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_countries", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "shipping",
                table: "shipping_countries",
                columns: new[] { "Id", "CreatedAtUtc", "IsEnabled", "IsoCode", "NameEn", "NameHy", "SortOrder", "UpdatedAtUtc" },
                values: new object[] { new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc), true, "AM", "Armenia", "Հայաստան", 0, new DateTime(2026, 7, 21, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_shipping_countries_IsEnabled",
                schema: "shipping",
                table: "shipping_countries",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_shipping_countries_IsoCode",
                schema: "shipping",
                table: "shipping_countries",
                column: "IsoCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipping_countries_SortOrder",
                schema: "shipping",
                table: "shipping_countries",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shipping_countries",
                schema: "shipping");
        }
    }
}
