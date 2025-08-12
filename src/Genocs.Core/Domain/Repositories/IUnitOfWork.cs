namespace Genocs.Core.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> Save();
}