using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Ocelot.Middleware;
using Ocelot.Multiplexer;

namespace APIGateway.Aggregators;

internal class RouteKeysAggregator : IDefinedAggregator
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope",
        Justification = "StringContent is returned inside DownstreamResponse and should not be disposed here.")]
    public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
    {
        var aggregate = new Dictionary<string, object>(responses.Count);
        foreach (var response in responses)
        {
            aggregate[response.Items.DownstreamRoute().Key] =
                await response.Items.DownstreamResponse().Content.ReadFromJsonAsync<object>();
        }

        var stringContent = new StringContent(JsonSerializer.Serialize(aggregate), Encoding.UTF8, MediaTypeNames.Application.Json);

        return new DownstreamResponse(
            stringContent,
            HttpStatusCode.OK,
            new List<KeyValuePair<string, IEnumerable<string>>>(),
            nameof(HttpStatusCode.OK));
    }
}
