using RestSharp;

namespace Beta.Lib;

public static class ApiClient
{
    // Uses the (intentionally vulnerable) RestSharp 106.6.7 dependency so it
    // appears in the restored dependency graph that Snyk Open Source reads.
    public static string Ping(string resource)
    {
        var client = new RestClient("https://example.com/" + resource);
        return client.BaseUrl?.ToString() ?? string.Empty;
    }
}
