using Genocs.Persistence.EFCore.Common;
using Genocs.Persistence.EFCore.Configurations;
using Genocs.Persistence.EFCore.Providers;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.MongoDB;

/// <summary>
/// MongoDB implementation of <see cref="IEFCoreDbProvider"/>.
/// The MongoDB EF Core provider does not support migrations, so initialization only runs seeders.
/// </summary>
public sealed class MongoDbProvider : IEFCoreDbProvider
{
    private const string HiddenValueDefault = "*******";

    public string ProviderKey => DbProviderKeys.MongoDB;

    public bool SupportsMigrations => false;

    public void Configure(DbContextOptionsBuilder builder, DatabaseOptions options)
        => builder.UseMongoDB(
            options.ConnectionString,
            options.GetMongoDatabaseName()
                ?? throw new InvalidOperationException("MongoDB database name must be configured in DatabaseOptions.DatabaseName or included in the connection string path."));

    public string MakeSecureConnectionString(string connectionString)
    {
        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            && uri.Scheme.StartsWith("mongodb", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(uri.UserInfo))
        {
            var builder = new UriBuilder(uri);
            string[]? userInfoParts = uri.UserInfo.Split(':', 2);

            string? maskedUserName = string.IsNullOrEmpty(userInfoParts[0])
                ? string.Empty
                : HiddenValueDefault;

            builder.UserName = maskedUserName;
            builder.Password = userInfoParts.Length == 2 && !string.IsNullOrEmpty(userInfoParts[1])
                ? HiddenValueDefault
                : string.Empty;

            return builder.Uri.ToString();
        }

        // Fallback for valid MongoDB connection strings that Uri cannot parse (e.g. multi-host authority).
        int schemeDelimiterIndex = connectionString.IndexOf("://", StringComparison.Ordinal);
        if (schemeDelimiterIndex < 0)
        {
            return connectionString;
        }

        int authorityStart = schemeDelimiterIndex + 3;
        int authorityEnd = connectionString.IndexOfAny(new[] { '/', '?', '#' }, authorityStart);
        if (authorityEnd < 0)
        {
            authorityEnd = connectionString.Length;
        }

        int atIndex = connectionString.LastIndexOf('@', authorityEnd - 1, authorityEnd - authorityStart);
        if (atIndex < 0)
        {
            return connectionString;
        }

        string credentials = connectionString.Substring(authorityStart, atIndex - authorityStart);
        if (string.IsNullOrEmpty(credentials))
        {
            return connectionString;
        }

        int separatorIndex = credentials.IndexOf(':');
        string maskedCredentials;
        if (separatorIndex >= 0)
        {
            string username = credentials[..separatorIndex];
            string password = credentials[(separatorIndex + 1)..];

            string maskedUserName = string.IsNullOrEmpty(username) ? string.Empty : HiddenValueDefault;
            string maskedPassword = string.IsNullOrEmpty(password) ? string.Empty : HiddenValueDefault;

            maskedCredentials = $"{maskedUserName}:{maskedPassword}";
        }
        else
        {
            maskedCredentials = HiddenValueDefault;
        }

        return string.Concat(
            connectionString.AsSpan(0, authorityStart),
            maskedCredentials,
            connectionString.AsSpan(atIndex));
    }

    public bool TryValidateConnectionString(string connectionString)
        => true;
}
