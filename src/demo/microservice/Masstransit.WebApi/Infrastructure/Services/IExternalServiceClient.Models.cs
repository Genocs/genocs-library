using Newtonsoft.Json;

namespace Genocs.Library.Demo.Masstransit.WebApi.Infrastructure.Services;

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
public class RedemptionRequest;

/// <summary>
/// The metadata object for the issuing endpoint.
/// It contains additional information about the request,
/// such as the member ID, request ID, and email address of the recipient.
/// </summary>
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
    public int? DiscountAmount { get; set; }

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

public class CustomSettingsData;

public class GiftCard
{
    [JsonProperty("barcode_format")]
    public string? BarcodeFormat { get; set; }

    [JsonProperty("countries")]
    public List<string>? Countries { get; set; }

    [JsonProperty("currency")]
    public string? Currency { get; set; }

    [JsonProperty("custom_settings_data")]
    public CustomSettingsData? CustomSettingsData { get; set; }

    [JsonProperty("denomination_type")]
    public string? DenominationType { get; set; }

    [JsonProperty("denominations")]
    public List<string>? Denominations { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("expiration_policy")]
    public string? ExpirationPolicy { get; set; }

    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("image")]
    public object? Image { get; set; }

    [JsonProperty("maximum_value")]
    public string? MaximumValue { get; set; }

    [JsonProperty("minimum_value")]
    public string? MinimumValue { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("provider")]
    public string? Provider { get; set; }

    [JsonProperty("redeem_instructions_html")]
    public string? RedeemInstructionsHtml { get; set; }

    [JsonProperty("terms_and_conditions_html")]
    public string? TermsAndConditionsHtml { get; set; }

    [JsonProperty("terms_and_conditions_url")]
    public string? TermsAndConditionsUrl { get; set; }

    [JsonProperty("type")]
    public string? Type { get; set; }
}

public class IssuanceDetails
{
    [JsonProperty("custom_settings_data")]
    public CustomSettingsData? CustomSettingsData { get; set; }

    [JsonProperty("external_reference")]
    public string? ExternalReference { get; set; }

    [JsonProperty("loyalty_information")]
    public LoyaltyInformation? LoyaltyInformation { get; set; }

    // public Metadata metadata { get; set; }
    [JsonProperty("original_external_reference")]
    public string? OriginalExternalReference { get; set; }

    [JsonProperty("original_code")]
    public string? OriginalCode { get; set; }

    [JsonProperty("partner_code")]
    public string? PartnerCode { get; set; }
}

public class LoyaltyInformation
{
    // public int? loyalty_amount { get; set; }
    [JsonProperty("loyalty_currency")]
    public string? LoyaltyCurrency { get; set; }

    [JsonProperty("member_id")]
    public string? MemberId { get; set; }

    [JsonProperty("partner_name")]
    public string? PartnerName { get; set; }
}