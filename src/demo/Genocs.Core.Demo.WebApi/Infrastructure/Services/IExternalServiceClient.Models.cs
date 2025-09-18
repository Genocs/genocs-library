using Newtonsoft.Json;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Services;

/// <summary>
/// The request object for the issuing endpoint.
/// </summary>
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

/// <summary>
/// The request object for the redemption endpoint.
/// </summary>
public class RedemptionRequest
{

}

public class Metadata
{
    [JsonProperty("memberId")]
    public string? MemberId { get; set; }

    [JsonProperty("requestId")]
    public string? RequestId { get; set; }

    [JsonProperty("email")]
    public string? Email { get; set; }
}

/// <summary>
/// The response object for the issuing endpoint.
/// </summary>
public class IssuingResponse
{
    [JsonProperty("code")]
    public CodeResponse? Code { get; set; }

    [JsonProperty("gift_card")]
    public GiftCard? GiftCard { get; set; }
}

/// <summary>
/// The Code data available in the response object.
/// </summary>
public class CodeResponse
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("code")]
    public string? Code { get; set; }

    [JsonProperty("barcode_string")]
    public string? Barcode { get; set; }

    [JsonProperty("barcode_format")]
    public string? BarcodeFormat { get; set; }

    [JsonProperty("discount_amount")]
    public int DiscountAmount { get; set; }

    [JsonProperty("delivery_url")]
    public object? DeliveryUrl { get; set; }

    [JsonProperty("expiration_date")]
    public string? ExpirationDate { get; set; }

    [JsonProperty("fiat_amount")]
    public string? FiatAmount { get; set; }

    [JsonProperty("fiat_balance")]
    public string? FiatBalance { get; set; }

    [JsonProperty("fiat_currency")]
    public string? FiatCurrency { get; set; }

    [JsonProperty("issuance_details")]
    public IssuanceDetails? IssuanceDetails { get; set; }

    [JsonProperty("pin")]
    public string? Pin { get; set; }

    [JsonProperty("redemption_details")]
    public object? RedemptionDetails { get; set; }

    [JsonProperty("redemption_status")]
    public string? RedemptionStatus { get; set; }
}

public class CustomSettingsData
{
}

public class GiftCard
{
    [JsonProperty("barcode_format")]
    public string? BarcodeFormat { get; set; }

    public List<string> countries { get; set; }
    public string currency { get; set; }
    public CustomSettingsData custom_settings_data { get; set; }
    public string denomination_type { get; set; }
    public List<string> denominations { get; set; }
    public string description { get; set; }

    // public int discount_value { get; set; }
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

    // public Metadata metadata { get; set; }
    public string original_external_reference { get; set; }
    public string original_code { get; set; }
    public string partner_code { get; set; }
}

public class LoyaltyInformation
{
    // public int? loyalty_amount { get; set; }
    public string loyalty_currency { get; set; }
    public string member_id { get; set; }
    public string partner_name { get; set; }
}