namespace Genocs.Template.Application.DTO;

/// <summary>
/// The UserDetails Dto
/// </summary>
public class UserDetailsDto : UserDto
{
    public string Email { get; set; }
    public string Role { get; set; }
    public IEnumerable<string>? Permissions { get; set; }
    public decimal Funds { get; set; }
}