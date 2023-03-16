namespace Genocs.WebApi.Requests;

public interface IRequestDispatcher
{
    Task<TResult> DispatchAsync<TRequest, TResult>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : class, IRequest;
}