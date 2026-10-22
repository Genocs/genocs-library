using System.ComponentModel.DataAnnotations;
using Genocs.Persistence.EFCore.Configurations;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Configurations;

public class DatabaseOptionsMongoDbTests
{
    [Fact]
    public void GetMongoDatabaseName_ReturnsConfiguredDatabaseName_WhenProvided()
    {
        var options = new DatabaseOptions
        {
            DBProvider = "mongodb",
            ConnectionString = "mongodb://localhost:27017/from-connection-string",
            DatabaseName = "configured-db"
        };

        options.GetMongoDatabaseName().ShouldBe("configured-db");
    }

    [Fact]
    public void GetMongoDatabaseName_ParsesDatabaseNameFromConnectionString_WhenNotConfigured()
    {
        var options = new DatabaseOptions
        {
            DBProvider = "mongodb",
            ConnectionString = "mongodb://localhost:27017/bookstore"
        };

        options.GetMongoDatabaseName().ShouldBe("bookstore");
    }

    [Fact]
    public void Validate_FailsForMongoDb_WhenDatabaseNameCannotBeResolved()
    {
        var options = new DatabaseOptions
        {
            DBProvider = "mongodb",
            ConnectionString = "mongodb://localhost:27017"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(options);
        var isValid = Validator.TryValidateObject(options, context, results, validateAllProperties: true);

        isValid.ShouldBeFalse();
        results.Any(r => r.ErrorMessage is not null && r.ErrorMessage.Contains(nameof(DatabaseOptions.DatabaseName), StringComparison.Ordinal))
            .ShouldBeTrue();
    }
}
