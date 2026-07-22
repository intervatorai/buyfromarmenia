using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsolidationShippingSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "settlements");

            migrationBuilder.EnsureSchema(
                name: "shipping");

            migrationBuilder.AddColumn<Guid>(
                name: "ConsolidationId",
                schema: "warehouse",
                table: "inbound_shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "consolidations",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consolidations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payouts",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    ScheduledForUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                schema: "shipping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsolidationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Carrier = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    TrackingNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    customs_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    customs_hs_code = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    customs_declared_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    customs_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier_settlements",
                schema: "settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformFee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EligibleAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_settlements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "consolidation_items",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsolidationId = table.Column<Guid>(type: "uuid", nullable: false),
                    InboundShipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consolidation_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_consolidation_items_consolidations_ConsolidationId",
                        column: x => x.ConsolidationId,
                        principalSchema: "warehouse",
                        principalTable: "consolidations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "packages",
                schema: "warehouse",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsolidationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_packages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_packages_consolidations_ConsolidationId",
                        column: x => x.ConsolidationId,
                        principalSchema: "warehouse",
                        principalTable: "consolidations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_shipments_ConsolidationId",
                schema: "warehouse",
                table: "inbound_shipments",
                column: "ConsolidationId");

            migrationBuilder.CreateIndex(
                name: "IX_consolidation_items_ConsolidationId",
                schema: "warehouse",
                table: "consolidation_items",
                column: "ConsolidationId");

            migrationBuilder.CreateIndex(
                name: "IX_consolidation_items_InboundShipmentId",
                schema: "warehouse",
                table: "consolidation_items",
                column: "InboundShipmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_consolidations_CustomerOrderId",
                schema: "warehouse",
                table: "consolidations",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_consolidations_ReferenceNumber",
                schema: "warehouse",
                table: "consolidations",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_packages_ConsolidationId",
                schema: "warehouse",
                table: "packages",
                column: "ConsolidationId");

            migrationBuilder.CreateIndex(
                name: "IX_payouts_SupplierId",
                schema: "settlements",
                table: "payouts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_ConsolidationId",
                schema: "shipping",
                table: "shipments",
                column: "ConsolidationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipments_CustomerOrderId",
                schema: "shipping",
                table: "shipments",
                column: "CustomerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_shipments_ReferenceNumber",
                schema: "shipping",
                table: "shipments",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shipments_TrackingNumber",
                schema: "shipping",
                table: "shipments",
                column: "TrackingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_settlements_SupplierId",
                schema: "settlements",
                table: "supplier_settlements",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_settlements_SupplierOrderId",
                schema: "settlements",
                table: "supplier_settlements",
                column: "SupplierOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "consolidation_items",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "packages",
                schema: "warehouse");

            migrationBuilder.DropTable(
                name: "payouts",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "shipments",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "supplier_settlements",
                schema: "settlements");

            migrationBuilder.DropTable(
                name: "consolidations",
                schema: "warehouse");

            migrationBuilder.DropIndex(
                name: "IX_inbound_shipments_ConsolidationId",
                schema: "warehouse",
                table: "inbound_shipments");

            migrationBuilder.DropColumn(
                name: "ConsolidationId",
                schema: "warehouse",
                table: "inbound_shipments");
        }
    }
}
