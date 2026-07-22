using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BFA.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtendCatalogAndSupplierDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommissionPlanId",
                schema: "suppliers",
                table: "suppliers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_city",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_country_code",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(2)",
                maxLength: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_line1",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_line2",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_postal_code",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "warehouse_region",
                schema: "suppliers",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BrandId",
                schema: "catalog",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductType",
                schema: "catalog",
                table: "products",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "shipping_contains_alcohol",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "shipping_contains_battery",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "shipping_contains_liquid",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shipping_dangerous_goods_code",
                schema: "catalog",
                table: "products",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "shipping_dimension_unit",
                schema: "catalog",
                table: "products",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_gross_weight",
                schema: "catalog",
                table: "products",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_height",
                schema: "catalog",
                table: "products",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "shipping_is_fragile",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "shipping_is_perishable",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_length",
                schema: "catalog",
                table: "products",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_net_weight",
                schema: "catalog",
                table: "products",
                type: "numeric(10,3)",
                precision: 10,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "shipping_requires_cooling",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "shipping_width",
                schema: "catalog",
                table: "products",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                schema: "catalog",
                table: "product_translations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoDescription",
                schema: "catalog",
                table: "product_translations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeoTitle",
                schema: "catalog",
                table: "product_translations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                schema: "catalog",
                table: "product_translations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsageInstructions",
                schema: "catalog",
                table: "product_translations",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_documents",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_documents_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_media",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaType = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AltText = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_media_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierSku = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Size = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Color = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Weight = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    length = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    width = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    height = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    dimension_unit = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CustomsCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    CountryOfOrigin = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_variants_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_bank_accounts",
                schema: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    account_holder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iban = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    swift = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_bank_accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_bank_accounts_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "suppliers",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_documents",
                schema: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    FileName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_documents_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "suppliers",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "supplier_members",
                schema: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    InvitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_members_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "suppliers",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "category_translations",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_category_translations_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "catalog",
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_translations_CategoryId_language_code",
                schema: "catalog",
                table: "category_translations",
                columns: new[] { "CategoryId", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_translations_Slug_language_code",
                schema: "catalog",
                table: "category_translations",
                columns: new[] { "Slug", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_documents_ProductId",
                schema: "catalog",
                table: "product_documents",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_media_ProductId",
                schema: "catalog",
                table: "product_media",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_variants_ProductId_SupplierSku",
                schema: "catalog",
                table: "product_variants",
                columns: new[] { "ProductId", "SupplierSku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_bank_accounts_SupplierId",
                schema: "suppliers",
                table: "supplier_bank_accounts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_SupplierId",
                schema: "suppliers",
                table: "supplier_documents",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_members_SupplierId_Email",
                schema: "suppliers",
                table: "supplier_members",
                columns: new[] { "SupplierId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_translations",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_documents",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_media",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "product_variants",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "supplier_bank_accounts",
                schema: "suppliers");

            migrationBuilder.DropTable(
                name: "supplier_documents",
                schema: "suppliers");

            migrationBuilder.DropTable(
                name: "supplier_members",
                schema: "suppliers");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "catalog");

            migrationBuilder.DropColumn(
                name: "CommissionPlanId",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "warehouse_city",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "warehouse_country_code",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "warehouse_line1",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "warehouse_line2",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "warehouse_postal_code",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "warehouse_region",
                schema: "suppliers",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "BrandId",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ProductType",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_contains_alcohol",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_contains_battery",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_contains_liquid",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_dangerous_goods_code",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_dimension_unit",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_gross_weight",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_height",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_is_fragile",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_is_perishable",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_length",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_net_weight",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_requires_cooling",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "shipping_width",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Ingredients",
                schema: "catalog",
                table: "product_translations");

            migrationBuilder.DropColumn(
                name: "SeoDescription",
                schema: "catalog",
                table: "product_translations");

            migrationBuilder.DropColumn(
                name: "SeoTitle",
                schema: "catalog",
                table: "product_translations");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                schema: "catalog",
                table: "product_translations");

            migrationBuilder.DropColumn(
                name: "UsageInstructions",
                schema: "catalog",
                table: "product_translations");
        }
    }
}
