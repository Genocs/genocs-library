using Genocs.Common.Persistence;
using Genocs.Persistence.EFCore.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
#if !NET10_0_OR_GREATER
using MySqlConnector;
#endif
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace Genocs.Persistence.EFCore.Configurations;

public class ConnectionStringSecurer : IConnectionStringSecurer
{
    private const string HiddenValueDefault = "*******";
    private readonly DatabaseOptions _dbSettings;

    public ConnectionStringSecurer(IOptions<DatabaseOptions> dbSettings) =>
        _dbSettings = dbSettings.Value;

    public string? MakeSecure(string? connectionString, string? dbProvider)
    {
        if (connectionString == null || string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        if (string.IsNullOrWhiteSpace(dbProvider))
        {
            dbProvider = _dbSettings.DBProvider;
        }

        return dbProvider?.ToLower() switch
        {
            DbProviderKeys.MongoDB => MakeSecureMongoDBConnectionString(connectionString),
            DbProviderKeys.MySql => MakeSecureMySqlConnectionString(connectionString),
            DbProviderKeys.Npgsql => MakeSecureNpgsqlConnectionString(connectionString),
            DbProviderKeys.Oracle => MakeSecureOracleConnectionString(connectionString),
            DbProviderKeys.SqlServer => MakeSecureSqlConnectionString(connectionString),
            DbProviderKeys.SqLite => MakeSecureSqLiteConnectionString(connectionString),
            _ => connectionString
        };
    }

    private static string MakeSecureMongoDBConnectionString(string connectionString)
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

#if !NET10_0_OR_GREATER
    private static string MakeSecureMySqlConnectionString(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }
#else
    private static string MakeSecureMySqlConnectionString(string connectionString) =>
        connectionString; // TODO: Re-enable when Pomelo supports .NET 10
#endif

    private static string MakeSecureNpgsqlConnectionString(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) /* || !builder.IntegratedSecurity */)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.Username) /* || !builder.IntegratedSecurity */)
        {
            builder.Username = HiddenValueDefault;
        }

        return builder.ToString();
    }

    private static string MakeSecureOracleConnectionString(string connectionString)
    {
        var builder = new OracleConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password))
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID))
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    private static string MakeSecureSqlConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);

        if (!string.IsNullOrEmpty(builder.Password) || !builder.IntegratedSecurity)
        {
            builder.Password = HiddenValueDefault;
        }

        if (!string.IsNullOrEmpty(builder.UserID) || !builder.IntegratedSecurity)
        {
            builder.UserID = HiddenValueDefault;
        }

        return builder.ToString();
    }

    private static string MakeSecureSqLiteConnectionString(string connectionString)
    {
        var builder = new SqliteConnection(connectionString);

        return builder.ToString();
    }
}