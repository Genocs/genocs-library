namespace Genocs.Identities.Application.DTO;

/// <summary>
/// The UserDetails.
/// </summary>
public class UserDetailsDto : UserDto
{
    public string? Email { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public IEnumerable<string>? Permissions { get; set; }
}