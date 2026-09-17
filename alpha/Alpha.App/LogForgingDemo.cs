using Microsoft.Extensions.Logging;

namespace Alpha.App.Security
{
    // In-house helper the team considers safe for log output. Deliberately written so the
    // engine cannot infer sanitization from the body alone (no literal CRLF stripping) —
    // that is exactly the situation a Rule Extension custom sanitizer is meant to fix.
    public static class LogSanitizer
    {
        public static string Clean(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var trimmed = input.Trim();
            return trimmed.Length > 256 ? trimmed.Substring(0, 256) : trimmed;
        }
    }
}

namespace Alpha.App
{
    // WARNING: intentionally insecure. Exists only to exercise Snyk Code's C# LogForging rule.
    // Two independent sinks, kept in separate methods so Snyk reports them as two findings
    // rather than collapsing them into one.
    public static class LogForgingDemo
    {
        public static void Run(ILogger logger, string[] args)
        {
            LogForgingBaseline.LogUnsanitized(logger, args);
            LogViaInHouseSanitizer(logger, args);
        }

        // Call site B — routed through the in-house helper. Fully qualified at the call site
        // on purpose: the docs warn that `using`-imported sanitizers may not resolve.
        private static void LogViaInHouseSanitizer(ILogger logger, string[] args)
        {
            string raw = args.Length > 2 ? args[2] : Console.ReadLine() ?? string.Empty;
            var safeInput = Alpha.App.Security.LogSanitizer.Clean(raw);
            logger.LogError("Password reset requested by: " + safeInput);
        }
    }
}
