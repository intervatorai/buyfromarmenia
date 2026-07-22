using BFA.Modules.Catalog.Domain.Aggregates;
using BFA.Modules.Catalog.Domain.Enums;
using BFA.Modules.Catalog.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BFA.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly BfaDbContext _dbContext;

    public ProductRepository(BfaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        return await QueryWithDetails()
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Slug == normalized, cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var normalized = slug.Trim().ToLowerInvariant();
        var query = _dbContext.Products.AsNoTracking().Where(product => product.Slug == normalized);
        if (excludeProductId.HasValue)
        {
            query = query.Where(product => product.Id != excludeProductId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .OrderByDescending(product => product.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await SearchPublishedAsync(new ProductSearchCriteria(), cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchPublishedAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var term = criteria.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var ids = await SearchPublishedIdsAsync(criteria, term, cancellationToken);
            if (ids.Count == 0)
            {
                return [];
            }

            var products = await QueryWithDetails()
                .AsNoTracking()
                .Where(product => ids.Contains(product.Id))
                .ToListAsync(cancellationToken);

            var order = ids.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);
            return products
                .OrderBy(product => order[product.Id])
                .ToList();
        }

        var query = QueryWithDetails()
            .AsNoTracking()
            .Where(product => product.Status == ProductStatus.Published);

        if (criteria.CategoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == criteria.CategoryId.Value);
        }

        if (criteria.Tag is { } tag && tag != ProductTag.None)
        {
            query = query.Where(product => product.Tag == tag);
        }
        else if (criteria.FeaturedOnly)
        {
            query = query.Where(product => product.Tag != ProductTag.None);
        }

        var list = await query.ToListAsync(cancellationToken);
        list = list
            .OrderBy(product => TagRank(product.Tag))
            .ThenByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .ToList();

        if (criteria.Take is > 0)
        {
            list = list.Take(criteria.Take.Value).ToList();
        }

        return list;
    }

    public async Task<IReadOnlyList<Product>> GetBySupplierIdAsync(
        Guid supplierId,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .Where(product => product.SupplierId == supplierId)
            .OrderByDescending(product => product.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByStatusAsync(
        ProductStatus status,
        CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .AsNoTracking()
            .Where(product => product.Status == status)
            .OrderByDescending(product => product.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.Products.AddAsync(product, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        product.ClearDomainEvents();
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
        product.ClearDomainEvents();
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<Guid>> SearchPublishedIdsAsync(
        ProductSearchCriteria criteria,
        string term,
        CancellationToken cancellationToken)
    {
        var sql = """
            SELECT p."Id" AS "Id"
            FROM catalog.products AS p
            WHERE p."Status" = 'Published'
              AND (
                to_tsvector('simple', coalesce(p."SearchText", '')) @@ plainto_tsquery('simple', @term)
                OR p."SearchText" ILIKE '%' || @term || '%'
                OR coalesce(p."SearchKeywords", '') ILIKE '%' || @term || '%'
                OR similarity(coalesce(p."SearchText", ''), @term) > 0.15
              )
            """;

        var parameters = new List<NpgsqlParameter>
        {
            new("term", term)
        };

        if (criteria.CategoryId.HasValue)
        {
            sql += """ AND p."CategoryId" = @categoryId""";
            parameters.Add(new NpgsqlParameter("categoryId", criteria.CategoryId.Value));
        }

        if (criteria.Tag is { } tag && tag != ProductTag.None)
        {
            sql += """ AND p."Tag" = @tag""";
            parameters.Add(new NpgsqlParameter("tag", tag.ToString()));
        }
        else if (criteria.FeaturedOnly)
        {
            sql += """ AND p."Tag" <> 'None'""";
        }

        sql += """
             ORDER BY
               ts_rank(to_tsvector('simple', coalesce(p."SearchText", '')), plainto_tsquery('simple', @term)) DESC,
               CASE p."Tag"
                 WHEN 'Popular' THEN 0
                 WHEN 'Bestseller' THEN 1
                 WHEN 'New' THEN 2
                 ELSE 3
               END,
               coalesce(p."UpdatedAt", p."CreatedAt") DESC
            """;

        if (criteria.Take is > 0)
        {
            sql += """ LIMIT @take""";
            parameters.Add(new NpgsqlParameter("take", criteria.Take.Value));
        }

        return await _dbContext.Database
            .SqlQueryRaw<GuidIdRow>(sql, parameters.ToArray())
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    private IQueryable<Product> QueryWithDetails()
    {
        return _dbContext.Products
            .Include("_translations")
            .Include("_variants")
            .Include("_media.MediaAsset")
            .Include("_documents");
    }

    private static int TagRank(ProductTag tag) => tag switch
    {
        ProductTag.Popular => 0,
        ProductTag.Bestseller => 1,
        ProductTag.New => 2,
        _ => 3
    };

    private sealed class GuidIdRow
    {
        public Guid Id { get; set; }
    }
}
