using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CatalogService.API.IntegrationTests.GraphQL;

[Trait("Category", "IntegrationTests")]
[Collection(nameof(CatalogApiFactory))]
public class GraphQLTests(CatalogApiFactory factory) : IClassFixture<CatalogApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client = factory.CreateClient();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task GraphQL_SimpleQuery_ReturnsSuccess()
    {
        var query = """
            query Categories {
                categories(first: 3, order: [{ name: ASC }]) {
                    nodes {
                        name
                        imageUrl
                        id
                    }
                }
            }
            """;

        var response = await _client.PostAsJsonAsync("/graphql", new { query });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse>(JsonOptions);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GraphQL_ExcessiveDepth_ReturnsDepthError()
    {
        var query = """
        query Categories {
            categories(order: [{ name: ASC }]) {
                nodes {
                    name
                    imageUrl
                    id
                    parentCategory {
                        parentCategory {
                            parentCategory {
                                parentCategory {
                                    parentCategory {
                                        name
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        """;

        var response = await _client.PostAsJsonAsync("/graphql", new { query });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GraphQLResponse>(JsonOptions);

        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e =>
            e.Message.Contains("exceeds the max allowed execution depth", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class GraphQLResponse
    {
        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }

        [JsonPropertyName("errors")]
        public GraphQLError[]? Errors { get; set; }
    }

    private sealed class GraphQLError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
