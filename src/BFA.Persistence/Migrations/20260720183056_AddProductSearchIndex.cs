using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchKeywords",
                schema: "catalog",
                table: "products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SearchText",
                schema: "catalog",
                table: "products",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""CREATE EXTENSION IF NOT EXISTS pg_trgm;""");

            migrationBuilder.Sql(
                """
                UPDATE catalog.products AS p
                SET "SearchText" = LEFT(lower(trim(both from coalesce((
                    SELECT string_agg(
                        concat_ws(
                            ' ',
                            t."Name",
                            t."ShortDescription",
                            t."Description",
                            t."Ingredients",
                            t."SeoTitle"),
                        ' ')
                    FROM catalog.product_translations AS t
                    WHERE t."ProductId" = p."Id"
                ), '') || ' ' || coalesce(p."Slug", ''))), 4000),
                    "SearchKeywords" = LEFT(lower(trim(both from coalesce((
                    SELECT string_agg(DISTINCT token, ', ')
                    FROM (
                        SELECT unnest(
                            regexp_split_to_array(
                                lower(coalesce(t."Name", '') || ' ' || coalesce(t."ShortDescription", '')),
                                '[^[:alnum:]]+')) AS token
                        FROM catalog.product_translations AS t
                        WHERE t."ProductId" = p."Id"
                    ) AS tokens
                    WHERE length(token) > 2
                ), ''))), 500)
                WHERE coalesce(p."SearchText", '') = '';
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_products_SearchText_fts"
                ON catalog.products
                USING gin (to_tsvector('simple', coalesce("SearchText", '')));
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_products_SearchText_trgm"
                ON catalog.products
                USING gin ("SearchText" gin_trgm_ops);
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_products_SearchKeywords_trgm"
                ON catalog.products
                USING gin ("SearchKeywords" gin_trgm_ops)
                WHERE "SearchKeywords" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS catalog."IX_products_SearchKeywords_trgm";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS catalog."IX_products_SearchText_trgm";""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS catalog."IX_products_SearchText_fts";""");

            migrationBuilder.DropColumn(
                name: "SearchKeywords",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "SearchText",
                schema: "catalog",
                table: "products");
        }
    }
}
