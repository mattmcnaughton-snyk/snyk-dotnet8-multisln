namespace Alpha.App.Extensions
{
    // Stand-in for the customer's `ToJson()` helper. JSON string escaping turns CR and LF
    // into the two-character sequences \r and \n, so the serialized output cannot inject
    // new log lines — which makes this a legitimate custom sanitizer for Log Forging.
    public static class JsonExtensions
    {
        public static string ToJson(this object value)
        {
            return System.Text.Json.JsonSerializer.Serialize(value);
        }
    }
}
