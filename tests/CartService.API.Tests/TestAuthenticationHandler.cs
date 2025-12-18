using System.Security.Claims;
using System.Text.Encodings.Web;
using CartService.API.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CartService.API.Tests;

internal class TestAuthenticationHandler(
    IOptions<JwtAuthOptions> authOptions,
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string TestScheme = "TestScheme";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = authOptions.Value.RequiredClaims.Select(claim => new Claim(claim.Type, claim.Value))
            .Append(new Claim(authOptions.Value.PermissionClaims.Read.Type, authOptions.Value.PermissionClaims.Read.Value))
            .Append(new Claim(authOptions.Value.PermissionClaims.Create.Type, authOptions.Value.PermissionClaims.Create.Value))
            .Append(new Claim(authOptions.Value.PermissionClaims.Update.Type, authOptions.Value.PermissionClaims.Update.Value))
            .Append(new Claim(authOptions.Value.PermissionClaims.Delete.Type, authOptions.Value.PermissionClaims.Delete.Value));

        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, JwtBearerDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
