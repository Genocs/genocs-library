using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Security.Claims;

namespace Genocs.Library.Demo.WebApi.Securities;

/// <summary>
/// This class is responsible for configuring the security features of the application, such as authorization policies and requirements.
/// </summary>
public static class SecurityEndpointsModule
{
    public static class RoleType
    {
        public const string User = "user";
        public const string Admin = "admin";
    }

    public static class Policies
    {
        public const string UserOnly = "UserOnly";
        public const string AdminOnly = "AdminOnly";
        public const string UserOrAdmin = "UserOrAdmin";
        public const string Reader = "Reader";
        public const string Reader2 = "Reader2";
        public const string Reader3 = "Reader3";
        public const string Reader4 = "Reader4";
    }

    public static IServiceCollection MapSecurityFeatures(this IServiceCollection services)
    {
        // Override the default authorization policy
        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Reader, builder
                => builder.RequireAssertion(context => context.User.HasClaim(ClaimTypes.Role, RoleType.User)))
            .AddPolicy(Policies.Reader2, builder
                => builder.RequireClaim(ClaimTypes.Role, RoleType.User))
            .AddPolicy(Policies.Reader3, builder
                => builder.RequireRole(RoleType.User))
            .AddPolicy(Policies.Reader4, builder
                => builder.AddRequirements(new AssertionRequirement(context => context.User.IsInRole(RoleType.User))));

        // services.AddAuthorizationBuilder()
        //                    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        //                    .RequireAuthenticatedUser()
        //                    .Build())
        //                        .AddPolicy(Policies.UserOnly, policy => policy.RequireAssertion(context
        //                            => context.User.HasClaim(ClaimTypes.Role, RoleType.User)))
        //                        .AddPolicy(Policies.AdminOnly, policy => policy.RequireAssertion(context
        //                            => context.User.HasClaim(ClaimTypes.Role, RoleType.Admin)))
        //                        .AddPolicy(Policies.UserOrAdmin, policy => policy.RequireAssertion(context
        //                            => context.User.HasClaim(ClaimTypes.Role, RoleType.User)
        //                                || context.User.HasClaim(ClaimTypes.Role, RoleType.Admin)));

        return services;
    }
}