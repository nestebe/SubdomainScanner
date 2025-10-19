using System.Net.Http;

namespace SubdomainScanner.Avalonia.Services;

public class SimpleHttpClientFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        // Create a new HttpClient instance for each request
        // This allows each scan to configure its own headers and timeout
        return new HttpClient();
    }
}
