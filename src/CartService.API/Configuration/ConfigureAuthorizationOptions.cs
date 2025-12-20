using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CartService.API.Configuration;

internal class ConfigureAuthorizationOptions(IOptions<JwtAuthOptions> authOptions)
    : IConfigureOptions<AuthorizationOptions>
{
    public void Configure(AuthorizationOptions options)
    {
        void AddPermissionPolicy(string policyName, RequiredClaim claim)
        {
            options.AddPolicy(policyName, p =>
            {
                p.RequireAuthenticatedUser();

                foreach (var requiredScope in authOptions.Value.RequiredClaims)
                    p.RequireClaim(requiredScope.Type, requiredScope.Value);

                p.RequireClaim(claim.Type, claim.Value);
            });
        }

        AddPermissionPolicy(nameof(Permissions.Read), authOptions.Value.PermissionClaims.Read);
        AddPermissionPolicy(nameof(Permissions.Create), authOptions.Value.PermissionClaims.Create);
        AddPermissionPolicy(nameof(Permissions.Update), authOptions.Value.PermissionClaims.Update);
        AddPermissionPolicy(nameof(Permissions.Delete), authOptions.Value.PermissionClaims.Delete);

        var builder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        foreach (var claim in authOptions.Value.RequiredClaims)
            builder.RequireClaim(claim.Type, claim.Value);

        options.DefaultPolicy = builder.Build();
        options.FallbackPolicy = options.DefaultPolicy;
    }
}
