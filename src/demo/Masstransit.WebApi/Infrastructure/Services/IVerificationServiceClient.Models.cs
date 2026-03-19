using Newtonsoft.Json;

namespace Genocs.Library.Demo.Masstransit.WebApi.Infrastructure.Services;

/// <summary>
/// The Internal User verification request.
/// </summary>
public class VerificationApiRequest
{
    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("joinedDate")]
    public string? JoinedDate { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("personDetails")]
    public PersonDetailsRequest? Details { get; set; }
}

public class PersonDetailsRequest
{
    [JsonProperty("firstName")]
    public string? FirstName { get; set; }

    [JsonProperty("lastName")]
    public string? LastName { get; set; }

    [JsonProperty("dob")]
    public string? DateOfBirth { get; set; }

    [JsonProperty("gender")]
    public string? Gender { get; set; }

    [JsonProperty("nationality")]
    public string? Nationality { get; set; }
}

/// <summary>
/// The internal User verification response.
/// </summary>
public class VerificationApiResponse
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public string? UpdatedAt { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }

    [JsonProperty("joinedDate")]
    public string? JoinedDate { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("personDetails")]
    public PersonDetailsRequest? Details { get; set; }
}