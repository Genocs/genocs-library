using Genocs.Common.Types;

namespace Genocs.Core.Domain.Entities;

/// <summary>
/// A shortcut of <see cref="IEntity{TPrimaryKey}"/> for most used primary key type (<see cref="int"/>).
/// </summary>
public interface IEntity : IIdentifiable<int>
{

}