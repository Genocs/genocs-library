using System.Linq.Expressions;

namespace Genocs.Common.Interfaces;

/// <summary>
/// Interface for job service.
/// TODO: This interface is used to define the methods for job scheduling and management.
/// Will be used in the future.
/// </summary>
public interface IJobService : ITransientService
{
    string Enqueue(Expression<Action> methodCall);

    string Enqueue(Expression<Func<Task>> methodCall);

    string Enqueue<T>(Expression<Action<T>> methodCall);

    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    string Schedule(Expression<Action> methodCall, TimeSpan delay);

    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    string Schedule(Expression<Action> methodCall, DateTimeOffset enqueueAt);

    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);

    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);

    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    bool Delete(string jobId);

    bool Delete(string jobId, string fromState);

    bool Requeue(string jobId);

    bool Requeue(string jobId, string fromState);
}