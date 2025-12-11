using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CartService.API.Configuration;

internal class ConfigureJwtBearerOptions(IOptions<JwtAuthOptions> authOptions) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
            return;

        var auth = authOptions.Value;

        options.Authority = auth.Authority;
        options.Audience = auth.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuers = auth.ValidIssuers,
            ValidAudiences = auth.ValidAudiences,
            ClockSkew = TimeSpan.FromMinutes(auth.ClockSkewMinutes)
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);
}