using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCustomerTrackingStage : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE ordering.customer_orders
              ADD COLUMN IF NOT EXISTS "TrackingStage" character varying(32) NOT NULL DEFAULT 'OrderPlaced';

            UPDATE ordering.customer_orders
            SET "TrackingStage" = CASE
              WHEN "Status" = 'Completed' THEN 'Delivered'
              WHEN "Status" = 'Confirmed' THEN 'Confirmed'
              ELSE 'OrderPlaced'
            END
            WHERE "TrackingStage" = 'OrderPlaced';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE ordering.customer_orders DROP COLUMN IF EXISTS "TrackingStage";
            """);
    }
}
