using BFA.Modules.Catalog.Domain.Enums;

namespace BFA.Modules.Catalog.Domain.Repositories;

public sealed record ProductSearchCriteria(
    Guid? CategoryId = null,
    string? Search = null,
    ProductTag? Tag = null,
    bool FeaturedOnly = false,
    int? Take = null);
