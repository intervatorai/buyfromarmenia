namespace BFA.Persistence;

/// <summary>
/// Railway Postgres exposes <c>DATABASE_URL</c> as <c>postgresql://...</c>.
/// Npgsql / Hangfire expect ADO.NET key=value form unless converted.
/// </summary>
public static class PostgresConnectionString
{
    public static string Normalize(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is missing or empty.");
        }

        var value = connectionString.Trim().Trim('"');

        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            && !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : string.Empty;
        var database = Uri.UnescapeDataString(uri.AbsolutePath.Trim('/'));
        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var sslMode = ResolveSslMode(uri.Query);

        return string.Join(
            ';',
            $"Host={uri.Host}",
            $"Port={port}",
            $"Database={database}",
            $"Username={username}",
            $"Password={password}",
            $"SSL Mode={sslMode}",
            "Trust Server Certificate=true");
    }

    private static string ResolveSslMode(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Require";
        }

        foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            if (pair.Length != 2)
            {
                continue;
            }

            if (!pair[0].Equals("sslmode", StringComparison.OrdinalIgnoreCase)
                && !pair[0].Equals("Ssl Mode", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return Uri.UnescapeDataString(pair[1]) switch
            {
                "disable" => "Disable",
                "allow" => "Allow",
                "prefer" => "Prefer",
                "require" => "Require",
                "verify-ca" => "VerifyCA",
                "verify-full" => "VerifyFull",
                var other => other
            };
        }

        return "Require";
    }
}
