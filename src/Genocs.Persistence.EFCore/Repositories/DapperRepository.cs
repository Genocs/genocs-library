using System.Data;
using Genocs.Core.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.EFCore.Context;

namespace Genocs.Persistence.EFCore.Repositories;
/*
public class DapperRepository : IDapperRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DapperRepository(ApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<IReadOnlyList<T>> QueryAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
        => (await _dbContext.Connection.QueryAsync<T>(sql, param, transaction))
            .AsList();

    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        //if (_dbContext.Model.GetMultiTenantEntityTypes().Any(t => t.ClrType == typeof(T)))
        //{
        //    sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
        //}

        return await _dbContext.Connection.QueryFirstOrDefaultAsync<T>(sql, param, transaction);
    }

    public Task<T> QuerySingleAsync<T>(string sql, object? param = null, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
        where T : class, IEntity
    {
        //if (_dbContext.Model.GetMultiTenantEntityTypes().Any(t => t.ClrType == typeof(T)))
        //{
        //    sql = sql.Replace("@tenant", _dbContext.TenantInfo.Id);
        //}

        return _dbContext.Connection.QuerySingleAsync<T>(sql, param, transaction);
    }
}

*/