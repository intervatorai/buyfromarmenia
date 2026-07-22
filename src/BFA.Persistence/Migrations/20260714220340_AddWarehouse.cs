using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "warehouse");

            migrationBuilder.CreateTable(
                name: "inbound_shipments",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ItemsCount = table.Column<int>(type: "integer", nullable: false),
                    receipt_scan_reference = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    receipt_weight_kg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: true),
                    receipt_inspection_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    receipt_photo_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    receipt_received_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    receipt_received_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_shipments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_shipments_ReferenceNumber",
                schema: "warehouse",
                table: "inbound_shipments",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbound_shipments_SupplierId",
                schema: "warehouse",
                table: "inbound_shipments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_shipments_SupplierOrderId",
                schema: "warehouse",
                table: "inbound_shipments",
                column: "SupplierOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbound_shipments",
                schema: "warehouse");
        }
    }
}
