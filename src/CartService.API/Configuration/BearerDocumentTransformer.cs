using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace CartService.API.Configuration;

internal sealed class BearerDocumentTransformer : IOpenApiDocumentTransformer
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
                In = ParameterLocation.Header
            };

        return Task.CompletedTask;
    }
}

internal sealed class SwaggerDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

        var swaggerOptions = context.ApplicationServices.GetService<IOptions<SwaggerOptions>>()?.Value ?? new();
        var original = new Uri(document.Servers[0].Url);
        var builder = new UriBuilder(original)
        {
            Scheme = swaggerOptions.Scheme ?? original.Scheme,
            Port = swaggerOptions.Port ?? original.Port
        };
        document.Servers[0].Url = builder.Uri.ToString();

        document.Components.SecuritySchemes["oauth2"] =
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(swaggerOptions.AuthorizationUrl),
                            TokenUrl = new Uri(swaggerOptions.TokenUrl),
                            Scopes = swaggerOptions.Scopes.ToDictionary(i => i, i => string.Empty)
                        }
                    }
                };

        return Task.CompletedTask;
    }
}
