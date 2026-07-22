using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaAssetsLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_media_ProductId",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.CreateTable(
                name: "media_assets",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ByteSize = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media_assets", x => x.Id);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "MediaAssetId",
                schema: "catalog",
                table: "product_media",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                """
                INSERT INTO catalog.media_assets ("Id", "StorageKey", "ContentType", "ByteSize", "CreatedAt")
                SELECT
                    gen_random_uuid(),
                    CASE
                        WHEN pm."Url" ~* '^https?://'
                            THEN regexp_replace(pm."Url", '^https?://[^/]+/', '')
                        ELSE trim(both '/' from replace(pm."Url", '\', '/'))
                    END,
                    'image/jpeg',
                    NULL,
                    NOW() AT TIME ZONE 'utc'
                FROM catalog.product_media AS pm
                WHERE coalesce(trim(pm."Url"), '') <> ''
                  AND NOT EXISTS (
                      SELECT 1
                      FROM catalog.media_assets AS existing
                      WHERE existing."StorageKey" = CASE
                          WHEN pm."Url" ~* '^https?://'
                              THEN regexp_replace(pm."Url", '^https?://[^/]+/', '')
                          ELSE trim(both '/' from replace(pm."Url", '\', '/'))
                      END
                  );

                UPDATE catalog.product_media AS pm
                SET "MediaAssetId" = asset."Id"
                FROM catalog.media_assets AS asset
                WHERE asset."StorageKey" = CASE
                    WHEN pm."Url" ~* '^https?://'
                        THEN regexp_replace(pm."Url", '^https?://[^/]+/', '')
                    ELSE trim(both '/' from replace(pm."Url", '\', '/'))
                END;

                DELETE FROM catalog.product_media
                WHERE "MediaAssetId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "MediaAssetId",
                schema: "catalog",
                table: "product_media",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "MediaType",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.DropColumn(
                name: "Url",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.CreateIndex(
                name: "IX_product_media_MediaAssetId",
                schema: "catalog",
                table: "product_media",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_product_media_ProductId_MediaAssetId",
                schema: "catalog",
                table: "product_media",
                columns: new[] { "ProductId", "MediaAssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_media_assets_StorageKey",
                schema: "catalog",
                table: "media_assets",
                column: "StorageKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_product_media_media_assets_MediaAssetId",
                schema: "catalog",
                table: "product_media",
                column: "MediaAssetId",
                principalSchema: "catalog",
                principalTable: "media_assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_media_media_assets_MediaAssetId",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.DropIndex(
                name: "IX_product_media_MediaAssetId",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.DropIndex(
                name: "IX_product_media_ProductId_MediaAssetId",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.AddColumn<string>(
                name: "MediaType",
                schema: "catalog",
                table: "product_media",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Image");

            migrationBuilder.AddColumn<string>(
                name: "Url",
                schema: "catalog",
                table: "product_media",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE catalog.product_media AS pm
                SET "Url" = asset."StorageKey"
                FROM catalog.media_assets AS asset
                WHERE asset."Id" = pm."MediaAssetId";
                """);

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                schema: "catalog",
                table: "product_media");

            migrationBuilder.DropTable(
                name: "media_assets",
                schema: "catalog");

            migrationBuilder.CreateIndex(
                name: "IX_product_media_ProductId",
                schema: "catalog",
                table: "product_media",
                column: "ProductId");
        }
    }
}
