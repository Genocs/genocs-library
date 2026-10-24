namespace Genocs.Common.Domain.Entities.Auditing;

/// <summary>
/// This interface is implemented by entities which must be audited.
/// Related properties are automatically set when saving or updating <see cref="IEntity"/> objects.
/// </summary>
public interface IAudited : ICreationAudited, IModificationAudited;

/// <summary>
/// Adds navigation properties to <see cref="IAudited"/> interface for user.
/// </summary>
/// <typeparam name="TUser">Type of the user.</typeparam>
public interface IAudited<TUser> : IAudited, ICreationAudited<TUser>, IModificationAudited<TUser>
    where TUser : IEntity<DefaultIdType>;
