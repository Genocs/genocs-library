using Genocs.Identities.Application.Domain.Constants;

namespace Genocs.Identities.Application.Domain.Entities;

public static class Role
{

    public static bool IsValid(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return false;
        }

        role = role.ToLowerInvariant();

        return role == Roles.User || role == Roles.Admin;
    }

    public static bool IsValid(IEnumerable<string> roles)
        => roles.All(IsValid);
}