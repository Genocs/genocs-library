using Genocs.Core.Domain.Entities;
using Genocs.Identities.Application.Domain.Entities;

namespace Genocs.Identities.Application.Mongo.Documents;

public class RefreshTokenDocument : IEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }

    public RefreshTokenDocument()
    {
    }

    public RefreshTokenDocument(RefreshToken refreshToken)
    {
        Id = refreshToken.Id;
        UserId = refreshToken.UserId;
        Token = refreshToken.Token;
        CreatedAt = refreshToken.CreatedAt;
        RevokedAt = refreshToken.RevokedAt;
    }

    public RefreshToken ToEntity() => new(Id, UserId, Token, CreatedAt, RevokedAt);

    public bool IsTransient()
    {
        throw new NotImplementedException();
    }
}