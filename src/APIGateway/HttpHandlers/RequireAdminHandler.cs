using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using APIGateway.Configuration;
using Microsoft.Extensions.Options;

namespace APIGateway.HttpHandlers;

internal class RequireAdminHandler(IOptions<AdminRole> adminRole) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request?.Headers, nameof(request.Headers));
        var auth = request.Headers.Authorization;
        if (auth?.Scheme != "Bearer" || string.IsNullOrEmpty(auth.Parameter))
        {
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(auth.Parameter);
        var claims = jwt.Claims.ToImmutableHashSet();
        if (!adminRole.Value.RequiredClaims.All(rc => claims.Any(c => c.Type == rc.Type && c.Value == rc.Value)))
        {
            return new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
