using System.Linq.Expressions;
using Genocs.Common.CQRS.Queries;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Genocs.Persistence.MongoDB.Repositories;

public static class Pagination
{
    public static async Task<PagedResult<T>> PaginateAsync<T>(this IQueryable<T> collection, IPagedQuery query)
        => await collection.PaginateAsync(query.OrderBy, query.SortOrder, query.Page, query.Results);

    public static async Task<PagedResult<T>> PaginateAsync<T>(
                                                              this IQueryable<T> collection,
                                                              string? orderBy,
                                                              string? sortOrder,
                                                              int page,
                                                              int resultsPerPage)
    {
        if (resultsPerPage < 0)
        {
            resultsPerPage = 10;
        }

        bool isEmpty = !await collection.AnyAsync();
        if (isEmpty)
        {
            return PagedResult<T>.Empty;
        }

        int totalResults = await collection.CountAsync();
        int totalPages = (int)Math.Ceiling((decimal)totalResults / resultsPerPage);

        List<T> data;
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            data = await collection.Limit(page, resultsPerPage).ToListAsync();
            return PagedResult<T>.Create(data, page, resultsPerPage, totalPages, totalResults);
        }

        if (sortOrder?.ToLowerInvariant() == "asc")
        {
            data = await collection.OrderBy(ToLambda<T>(orderBy)).Limit(page, resultsPerPage).ToListAsync();
        }
        else
        {
            data = await collection.OrderByDescending(ToLambda<T>(orderBy)).Limit(page, resultsPerPage).ToListAsync();
        }

        return PagedResult<T>.Create(data, page, resultsPerPage, totalPages, totalResults);
    }

    public static IQueryable<T> Limit<T>(this IQueryable<T> collection, IPagedQuery query)
        => collection.Limit(query.Page, query.Results);

    public static IQueryable<T> Limit<T>(this IQueryable<T> collection, int page, int resultsPerPage)
    {
        if (page < 0)
        {
            page = 0;
        }

        if (resultsPerPage < 0)
        {
            resultsPerPage = 10;
        }

        var data = collection
                        .Skip(page * resultsPerPage)
                        .Take(resultsPerPage);

        return data;
    }

    private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(T));
        var property = Expression.Property(parameter, propertyName);
        var propAsObject = Expression.Convert(property, typeof(object));

        return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
    }
}