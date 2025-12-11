using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace CartService.API.Configuration;

internal sealed class SecurityOperationTransformer(IOptions<AuthenticationOptions> authOptions) : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Security ??= [];

        var document = context.Document;
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
        });

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("oauth2", document)] = [.. authOptions.Value.RequiredScopes.Select(s => s.FullName)]
        });

        return Task.CompletedTask;
    }
}