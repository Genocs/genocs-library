using Newtonsoft.Json;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Services;

public class IssuingRequest
{
    [JsonProperty("external_reference")]
    public string? ExternalReference;

    [JsonProperty("loyalty_currency")]
    public string? Currency;

    [JsonProperty("fiat_amount")]
    public decimal Amount;

    [JsonProperty("partner_code")]
    public string? PartnerCode;
}

public class Metadata
{
    public string memberId { get; set; }
    public string requestId { get; set; }
    public string email { get; set; }
}

public class IssuingResponse
{
    public Code code { get; set; }
    public GiftCard gift_card { get; set; }
}

public class Code
{
    public string barcode_format { get; set; }
    public string barcode_string { get; set; }
    public int discount_amount { get; set; }
    public object delivery_url { get; set; }
    public string expiration_date { get; set; }
    public string fiat_amount { get; set; }
    public string fiat_balance { get; set; }
    public string fiat_currency { get; set; }
    public string code { get; set; }
    public string id { get; set; }
    public IssuanceDetails issuance_details { get; set; }
    public string pin { get; set; }
    public object redemption_details { get; set; }
    public string redemption_status { get; set; }
}

public class CustomSettingsData
{
}

public class GiftCard
{
    public string barcode_format { get; set; }
    public List<string> countries { get; set; }
    public string currency { get; set; }
    public CustomSettingsData custom_settings_data { get; set; }
    public string denomination_type { get; set; }
    public List<string> denominations { get; set; }
    public string description { get; set; }

    //public int discount_value { get; set; }
    public string expiration_policy { get; set; }
    public string id { get; set; }
    public object image { get; set; }
    public string maximum_value { get; set; }
    public string minimum_value { get; set; }
    public string name { get; set; }
    public string provider { get; set; }
    public string redeem_instructions_html { get; set; }
    public string terms_and_conditions_html { get; set; }
    public string terms_and_conditions_url { get; set; }
    public string type { get; set; }
}

public class IssuanceDetails
{
    public CustomSettingsData custom_settings_data { get; set; }
    public string external_reference { get; set; }
    public LoyaltyInformation loyalty_information { get; set; }
//    public Metadata metadata { get; set; }
    public string original_external_reference { get; set; }
    public string original_code { get; set; }
    public string partner_code { get; set; }
}

public class LoyaltyInformation
{
    //public int? loyalty_amount { get; set; }
    public string loyalty_currency { get; set; }
    public string member_id { get; set; }
    public string partner_name { get; set; }
}

/// <summary>
/// The External WebApi client definition.
/// </summary>
public interface IExternalServiceClient
{
    /// <summary>
    /// Send a request for gift card issuing.
    /// </summary>
    /// <param name="request">The issuing Request.</param>
    /// <returns>The issuing Response.</returns>
    Task<IssuingResponse> IssueAsync(IssuingRequest request);

    /// <summary>
    /// Get the product based on the productId.
    /// </summary>
    /// <param name="callerId">The ProductId.</param>
    /// <returns>The Product Response.</returns>
    Task<string> RedeemAsync(string callerId);

}
