using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                schema: "catalog",
                table: "products",
                type: "character varying(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE catalog.products
                SET "Slug" = 'product-' || REPLACE("Id"::text, '-', '')
                WHERE "Slug" IS NULL OR "Slug" = '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_products_Slug",
                schema: "catalog",
                table: "products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_products_Slug",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Slug",
                schema: "catalog",
                table: "products");
        }
    }
}
