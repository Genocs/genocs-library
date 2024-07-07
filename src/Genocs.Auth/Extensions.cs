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
                                        string sectionName = JwtOptions.Position,
                                        Action<JwtBearerOptions>? optionsFactory = null)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = JwtOptions.Position;
        }

        var options = builder.GetOptions<JwtOptions>(sectionName);
        return builder.AddJwt(options, optionsFactory);
    }

    private static IGenocsBuilder AddJwt(
                                        this IGenocsBuilder builder,
                                        JwtOptions options,
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

        if (!options.Enabled)
        {
            builder.Services.AddSingleton<IPolicyEvaluator, DisabledAuthenticationPolicyEvaluator>();
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            RequireAudience = options.RequireAudience,
            ValidIssuer = options.ValidIssuer,
            ValidIssuers = options.ValidIssuers,
            ValidateActor = options.ValidateActor,
            ValidAudience = options.ValidAudience,
            ValidAudiences = options.ValidAudiences,
            ValidateAudience = options.ValidateAudience,
            ValidateIssuer = options.ValidateIssuer,
            ValidateLifetime = options.ValidateLifetime,
            ValidateTokenReplay = options.ValidateTokenReplay,
            ValidateIssuerSigningKey = options.ValidateIssuerSigningKey,
            SaveSigninToken = options.SaveSigninToken,
            RequireExpirationTime = options.RequireExpirationTime,
            RequireSignedTokens = options.RequireSignedTokens,
            ClockSkew = TimeSpan.Zero
        };

        if (!string.IsNullOrWhiteSpace(options.AuthenticationType))
        {
            tokenValidationParameters.AuthenticationType = options.AuthenticationType;
        }

        bool hasCertificate = false;
        if (options.Certificate is not null)
        {
            X509Certificate2? certificate = null;
            string? password = options.Certificate.Password;
            bool hasPassword = !string.IsNullOrWhiteSpace(password);
            if (!string.IsNullOrWhiteSpace(options.Certificate.Location))
            {
                certificate = hasPassword
                    ? new X509Certificate2(options.Certificate.Location, password)
                    : new X509Certificate2(options.Certificate.Location);
                string keyType = certificate.HasPrivateKey ? "with private key" : "with public key only";
                Console.WriteLine($"Loaded X.509 certificate from location: '{options.Certificate.Location}' {keyType}.");
            }

            if (!string.IsNullOrWhiteSpace(options.Certificate.RawData))
            {
                byte[] rawData = Convert.FromBase64String(options.Certificate.RawData);
                certificate = hasPassword
                    ? new X509Certificate2(rawData, password)
                    : new X509Certificate2(rawData);
                string keyType = certificate.HasPrivateKey ? "with private key" : "with public key only";
                Console.WriteLine($"Loaded X.509 certificate from raw data {keyType}.");
            }

            if (certificate is not null)
            {
                if (string.IsNullOrWhiteSpace(options.Algorithm))
                {
                    options.Algorithm = SecurityAlgorithms.RsaSha256;
                }

                hasCertificate = true;
                tokenValidationParameters.IssuerSigningKey = new X509SecurityKey(certificate);
                string actionType = certificate.HasPrivateKey ? "issuing" : "validating";
                Console.WriteLine($"Using X.509 certificate for {actionType} tokens.");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.IssuerSigningKey) && !hasCertificate)
        {
            if (string.IsNullOrWhiteSpace(options.Algorithm) || hasCertificate)
            {
                options.Algorithm = SecurityAlgorithms.HmacSha256;
            }

            byte[] rawKey = Encoding.UTF8.GetBytes(options.IssuerSigningKey);
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(rawKey);
            Console.WriteLine("Using symmetric encryption for issuing tokens.");
        }

        if (!string.IsNullOrWhiteSpace(options.NameClaimType))
        {
            tokenValidationParameters.NameClaimType = options.NameClaimType;
        }

        if (!string.IsNullOrWhiteSpace(options.RoleClaimType))
        {
            tokenValidationParameters.RoleClaimType = options.RoleClaimType;
        }

        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = options.Challenge;
                o.DefaultChallengeScheme = options.Challenge;
                o.DefaultScheme = options.Challenge;
            })
            .AddJwtBearer(o =>
            {
                o.Authority = options.Authority;
                o.Audience = options.Audience;
                o.MetadataAddress = options.MetadataAddress;
                o.SaveToken = options.SaveToken;
                o.RefreshOnIssuerKeyNotFound = options.RefreshOnIssuerKeyNotFound;
                o.RequireHttpsMetadata = options.RequireHttpsMetadata;
                o.IncludeErrorDetails = options.IncludeErrorDetails;
                o.TokenValidationParameters = tokenValidationParameters;
                if (!string.IsNullOrWhiteSpace(options.Challenge))
                {
                    o.Challenge = options.Challenge;
                }

                optionsFactory?.Invoke(o);
            });

        builder.Services.AddSingleton(options);
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
                                                string sectionName = JwtOptions.Position)
    {

        JwtOptions options = builder.Configuration.GetOptions<JwtOptions>(sectionName);

        string metadataAddress = $"{options.Issuer}{options.MetadataAddress}";
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(metadataAddress, new OpenIdConnectConfigurationRetriever());

        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = options.Challenge;
                o.DefaultChallengeScheme = options.Challenge;
                o.DefaultScheme = options.Challenge;
            })
            .AddJwtBearer(o =>
            {
                o.IncludeErrorDetails = options.IncludeErrorDetails;
                o.RefreshOnIssuerKeyNotFound = options.RefreshOnIssuerKeyNotFound;
                o.MetadataAddress = metadataAddress;
                o.ConfigurationManager = configurationManager;
                o.Audience = options.Audience;
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
                                    string sectionName = JwtOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = JwtOptions.Position;
        }

        JwtOptions options = builder.Configuration.GetOptions<JwtOptions>(sectionName);

        if (string.IsNullOrWhiteSpace(options.IssuerSigningKey))
        {
            throw new InvalidOperationException("Issuer signing key is missing.");
        }

        SecurityKey signingKey = SecurityKeyBuilder.CreateRsaSecurityKey(options.IssuerSigningKey);

        builder.Services
            .AddAuthentication(o =>
            {
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.SaveToken = options.SaveToken;
                o.RequireHttpsMetadata = options.RequireHttpsMetadata;
                o.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = signingKey,
                    ValidateAudience = options.ValidateAudience,
                    ValidAudience = options.ValidAudience,
                    ValidateIssuer = options.ValidateIssuer,
                    ValidIssuer = options.ValidIssuer,
                    ValidateLifetime = options.ValidateLifetime,
                    ValidateIssuerSigningKey = options.ValidateIssuerSigningKey
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