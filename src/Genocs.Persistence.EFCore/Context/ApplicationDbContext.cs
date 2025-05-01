using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Context;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    // Used by Dapper
    public IDbConnection Connection => Database.GetDbConnection();
}
