using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace CartService.API.Configuration;

internal sealed class SecurityDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] =
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter an existing JWT token"
            };

        var authOptions = context.ApplicationServices.GetRequiredService<IOptions<AuthenticationOptions>>();
        var swaggerOptions = context.ApplicationServices.GetService<IOptions<SwaggerOptions>>();

        if (swaggerOptions != null)
        {
            document.Components.SecuritySchemes["oauth2"] =
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(swaggerOptions.Value.AuthorizationUrl),
                            TokenUrl = new Uri(swaggerOptions.Value.TokenUrl),
                            Scopes = authOptions.Value.RequiredScopes.ToDictionary(i => i.FullName, i => i.Description ?? string.Empty)
                        }
                    }
                };
        }

        return Task.CompletedTask;
    }
}