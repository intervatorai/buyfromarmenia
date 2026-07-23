using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations;

/// <inheritdoc />
public partial class AddCategorySkuPrefix : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent: column may already exist if applied manually during rollout.
        migrationBuilder.Sql("""
            ALTER TABLE catalog.categories
              ADD COLUMN IF NOT EXISTS "SkuPrefix" character varying(4) NOT NULL DEFAULT '';

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_categories_SkuPrefix"
              ON catalog.categories ("SkuPrefix")
              WHERE "SkuPrefix" <> '';

            UPDATE catalog.categories AS c
            SET "SkuPrefix" = v.prefix
            FROM catalog.category_translations AS t
            JOIN (
                VALUES
                    ('food-grocery', 'FG'),
                    ('handicrafts', 'HC'),
                    ('textiles', 'TX'),
                    ('wine-spirits', 'WS'),
                    ('beauty-wellness', 'BW'),
                    ('jewelry', 'JW'),
                    ('home-decor', 'HD'),
                    ('souvenirs', 'SV')
            ) AS v(slug, prefix) ON t."Slug" = v.slug
            WHERE t."CategoryId" = c."Id"
              AND t.language_code = 'en'
              AND c."SkuPrefix" = '';
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DROP INDEX IF EXISTS catalog."IX_categories_SkuPrefix";
            ALTER TABLE catalog.categories DROP COLUMN IF EXISTS "SkuPrefix";
            """);
    }
}
