using Genocs.Auth.Configurations;
using Genocs.Auth.Handlers;
using Genocs.Auth.Services;
using Genocs.Core.Builders;
using Genocs.Security.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Genocs.Auth;

public static class Extensions
{
    private const string RegistryName = "auth";

    public static IGenocsBuilder AddJwt(
                                        this IGenocsBuilder builder,
                                        string sectionName = JwtSettings.Position,
                                        Action<JwtBearerOptions>? optionsFactory = null)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = JwtSettings.Position;
        }

        var options = builder.GetOptions<JwtSettings>(sectionName);
        return builder.AddJwt(options, optionsFactory);
    }

    private static IGenocsBuilder AddJwt(
                                        this IGenocsBuilder builder,
                                        JwtSettings jwtSettings,
                                        Action<JwtBearerOptions>? optionsFactory = null)
    {
        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IJwtHandler, JwtHandler>();
        builder.Services.AddSingleton<IAccessTokenService, InMemoryAccessTokenService>();
        builder.Services.AddTransient<AccessTokenValidatorMiddleware>();

        if (!jwtSettings.Enabled)
        {
            builder.Services.AddSingleton<IPolicyEvaluator, DisabledAuthenticationPolicyEvaluator>();
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            RequireAudience = jwtSettings.RequireAudience,
            ValidIssuer = jwtSettings.ValidIssuer,
            ValidIssuers = jwtSettings.ValidIssuers,
            ValidateActor = jwtSettings.ValidateActor,
            ValidAudience = jwtSettings.ValidAudience,
            ValidAudiences = jwtSettings.ValidAudiences,
            ValidateAudience = jwtSettings.ValidateAudience,
            ValidateIssuer = jwtSettings.ValidateIssuer,
            ValidateLifetime = jwtSettings.ValidateLifetime,
            ValidateTokenReplay = jwtSettings.ValidateTokenReplay,
            ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey,
            SaveSigninToken = jwtSettings.SaveSigninToken,
            RequireExpirationTime = jwtSettings.RequireExpirationTime,
            RequireSignedTokens = jwtSettings.RequireSignedTokens,
            ClockSkew = TimeSpan.Zero
        };

        if (!string.IsNullOrWhiteSpace(jwtSettings.AuthenticationType))
        {
            tokenValidationParameters.AuthenticationType = jwtSettings.AuthenticationType;
        }

        bool hasCertificate = false;
        if (jwtSettings.Certificate is not null)
        {
            X509Certificate2? certificate = null;
            string? password = jwtSettings.Certificate.Password;
            bool hasPassword = !string.IsNullOrWhiteSpace(password);
            if (!string.IsNullOrWhiteSpace(jwtSettings.Certificate.Location))
            {
                certificate = hasPassword
                    ? new X509Certificate2(jwtSettings.Certificate.Location, password)
                    : new X509Certificate2(jwtSettings.Certificate.Location);
                string keyType = certificate.HasPrivateKey ? "with private key" : "with public key only";
                Console.WriteLine($"Loaded X.509 certificate from location: '{jwtSettings.Certificate.Location}' {keyType}.");
            }

            if (!string.IsNullOrWhiteSpace(jwtSettings.Certificate.RawData))
            {
                byte[] rawData = Convert.FromBase64String(jwtSettings.Certificate.RawData);
                certificate = hasPassword
                    ? new X509Certificate2(rawData, password)
                    : new X509Certificate2(rawData);
                string keyType = certificate.HasPrivateKey ? "with private key" : "with public key only";
                Console.WriteLine($"Loaded X.509 certificate from raw data {keyType}.");
            }

            if (certificate is not null)
            {
                if (string.IsNullOrWhiteSpace(jwtSettings.Algorithm))
                {
                    jwtSettings.Algorithm = SecurityAlgorithms.RsaSha256;
                }

                hasCertificate = true;
                tokenValidationParameters.IssuerSigningKey = new X509SecurityKey(certificate);
                string actionType = certificate.HasPrivateKey ? "issuing" : "validating";
                Console.WriteLine($"Using X.509 certificate for {actionType} tokens.");
            }
        }

        if (!string.IsNullOrWhiteSpace(jwtSettings.IssuerSigningKey) && !hasCertificate)
        {
            if (string.IsNullOrWhiteSpace(jwtSettings.Algorithm) || hasCertificate)
            {
                jwtSettings.Algorithm = SecurityAlgorithms.HmacSha256;
            }

            byte[] rawKey = Encoding.UTF8.GetBytes(jwtSettings.IssuerSigningKey);
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(rawKey);
            Console.WriteLine("Using symmetric encryption for issuing tokens.");
        }

        if (!string.IsNullOrWhiteSpace(jwtSettings.NameClaimType))
        {
            tokenValidationParameters.NameClaimType = jwtSettings.NameClaimType;
        }

        if (!string.IsNullOrWhiteSpace(jwtSettings.RoleClaimType))
        {
            tokenValidationParameters.RoleClaimType = jwtSettings.RoleClaimType;
        }

        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = jwtSettings.Challenge;
                o.DefaultChallengeScheme = jwtSettings.Challenge;
                o.DefaultScheme = jwtSettings.Challenge;
            })
            .AddJwtBearer(o =>
            {
                o.Authority = jwtSettings.Authority;
                o.Audience = jwtSettings.Audience;
                o.MetadataAddress = jwtSettings.MetadataAddress;
                o.SaveToken = jwtSettings.SaveToken;
                o.RefreshOnIssuerKeyNotFound = jwtSettings.RefreshOnIssuerKeyNotFound;
                o.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                o.IncludeErrorDetails = jwtSettings.IncludeErrorDetails;
                o.TokenValidationParameters = tokenValidationParameters;
                if (!string.IsNullOrWhiteSpace(jwtSettings.Challenge))
                {
                    o.Challenge = jwtSettings.Challenge;
                }

                optionsFactory?.Invoke(o);
            });

        builder.Services.AddSingleton(jwtSettings);
        builder.Services.AddSingleton(tokenValidationParameters);

        return builder;
    }

    /// <summary>
    /// Enable OpenId Connect Authentication.
    /// It can be used with Firebase Authentication.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The configuration section name.</param>
    /// <returns>The Genocs builder you can use for chain.</returns>
    public static IGenocsBuilder AddOpenIdJwt(
                                                this IGenocsBuilder builder,
                                                string sectionName = JwtSettings.Position)
    {

        var jwtSettings = new JwtSettings();
        builder.Configuration.GetSection(sectionName).Bind(jwtSettings);

        string metadataAddress = $"{jwtSettings.Issuer}{jwtSettings.MetadataAddress}";
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever());

        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = jwtSettings.Challenge;
                o.DefaultChallengeScheme = jwtSettings.Challenge;
                o.DefaultScheme = jwtSettings.Challenge;
            })
            .AddJwtBearer(o =>
            {
                o.IncludeErrorDetails = jwtSettings.IncludeErrorDetails;
                o.RefreshOnIssuerKeyNotFound = jwtSettings.RefreshOnIssuerKeyNotFound;
                o.MetadataAddress = metadataAddress;
                o.ConfigurationManager = configurationManager;
                o.Audience = jwtSettings.Audience;
            });

        return builder;
    }

    /// <summary>
    /// It adds the private key JWT authentication.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="sectionName">The optional section name. Default name: 'jwt'.</param>
    /// <returns>The Genocs builder you can use for chaining.</returns>
    /// <exception cref="InvalidOperationException">Whenever mandatory data like 'IssuerSigningKey' is missing.</exception>
    public static IGenocsBuilder AddPrivateKeyJwt(
                                    this IGenocsBuilder builder,
                                    string sectionName = JwtSettings.Position)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = JwtSettings.Position;
        }

        var jwtSettings = new JwtSettings();
        builder.Configuration.GetSection(sectionName).Bind(jwtSettings);

        if (string.IsNullOrWhiteSpace(jwtSettings.IssuerSigningKey))
        {
            throw new InvalidOperationException("Issuer signing key is missing.");
        }

        SecurityKey signingKey = SecurityKeyBuilder.CreateRsaSecurityKey(jwtSettings.IssuerSigningKey);

        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = jwtSettings.SaveToken;
                o.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateAudience = jwtSettings.ValidateAudience,
                    ValidAudience = jwtSettings.ValidAudience,
                    ValidateIssuer = jwtSettings.ValidateIssuer,
                    ValidIssuer = jwtSettings.ValidIssuer,
                    ValidateLifetime = jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = jwtSettings.ValidateIssuerSigningKey
                };
            });

        return builder;
    }

    public static IApplicationBuilder UseAccessTokenValidator(this IApplicationBuilder app)
        => app.UseMiddleware<AccessTokenValidatorMiddleware>();
}

/// <summary>
/// DateExtensions extension method.
/// </summary>
internal static class DateExtensions
{
    /// <summary>
    /// ToTimestamp support function.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static long ToTimestamp(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();
}