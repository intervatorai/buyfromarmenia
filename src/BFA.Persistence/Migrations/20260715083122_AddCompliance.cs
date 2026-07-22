using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompliance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "compliance");

            migrationBuilder.CreateTable(
                name: "trade_restrictions",
                schema: "compliance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationCountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trade_restrictions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_trade_restrictions_DestinationCountryCode",
                schema: "compliance",
                table: "trade_restrictions",
                column: "DestinationCountryCode");

            migrationBuilder.CreateIndex(
                name: "IX_trade_restrictions_IsActive",
                schema: "compliance",
                table: "trade_restrictions",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trade_restrictions",
                schema: "compliance");
        }
    }
}
