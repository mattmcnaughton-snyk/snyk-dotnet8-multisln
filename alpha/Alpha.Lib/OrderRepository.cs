using Newtonsoft.Json;

namespace Alpha.Lib;

public static class OrderRepository
{
    // Uses the (intentionally vulnerable) Newtonsoft.Json 9.0.1 dependency so it
    // appears in the restored dependency graph that Snyk Open Source reads.
    public static string Describe()
    {
        var payload = new { Service = "Alpha", Orders = 0 };
        return JsonConvert.SerializeObject(payload);
    }
}
