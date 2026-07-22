using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BFA.Modules.Catalog.Domain;

public static class SlugHelper
{
    private static readonly Regex NonSlugChars = new(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled);
    private static readonly Regex MultiDash = new(@"-+", RegexOptions.Compiled);

    public static string From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Normalize(NormalizationForm.FormKD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(char.ToLowerInvariant(character));
        }

        var slug = NonSlugChars.Replace(builder.ToString(), "-");
        slug = MultiDash.Replace(slug, "-").Trim('-');
        return slug;
    }

    public static string EnsureUnique(string baseSlug, Func<string, bool> exists)
    {
        var slug = string.IsNullOrWhiteSpace(baseSlug) ? "product" : baseSlug;
        if (!exists(slug))
        {
            return slug;
        }

        for (var index = 2; index < 1000; index++)
        {
            var candidate = $"{slug}-{index}";
            if (!exists(candidate))
            {
                return candidate;
            }
        }

        return $"{slug}-{Guid.NewGuid():N}"[..Math.Min(80, slug.Length + 33)];
    }
}
