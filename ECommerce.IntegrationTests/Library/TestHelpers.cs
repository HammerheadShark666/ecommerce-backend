using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ECommerce.IntegrationTests.Library;

public static class TestHelpers
{
    public static TestApplicationFactory CreateFactoryFromFixture(SqlServerFixture fixture)
        => new(fixture.ConnectionString);

    public static HttpClient CreateClientWithAuth(this WebApplicationFactory<Api.AssemblyMarker> factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<ProblemDetails?> ParseProblemDetailsAsync(this HttpResponseMessage resp)
    {
        try
        {
            return await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        }
        catch
        {
            return null;
        }
    }

    public static HttpRequestMessage SetForwardedHeader(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation(
            "X-Forwarded-For",
            "192.168.1.100");

        return request;
    }
}
