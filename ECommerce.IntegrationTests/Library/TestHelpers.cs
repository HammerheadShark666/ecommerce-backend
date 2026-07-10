using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using ECommerce.Application.Abstractions;

namespace ECommerce.IntegrationTests.Library;

public static class TestHelpers
{
    public static TestApplicationFactory CreateFactoryFromFixture(SqlServerFixture fixture)
        => new(fixture.ConnectionString);

    public static HttpClient CreateClientWithAuth(this WebApplicationFactory<Api.AssemblyMarker> factory, string token)
    {
        HttpClient client = factory.CreateClient();
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
}
