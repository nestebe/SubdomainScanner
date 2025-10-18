using System.Net.Http;

namespace SubdomainScanner.Avalonia.Services;

public class SimpleHttpClientFactory : IHttpClientFactory
{
    private readonly HttpClient _httpClient = new();

    public HttpClient CreateClient(string name)
    {
        return _httpClient;
    }
}
