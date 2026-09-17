using Microsoft.Extensions.Logging;

namespace Alpha.App
{
    // Call site A — UNSANITIZED baseline LogForging sink, kept in its own file so Snyk
    // reports it separately from the sanitized call site in LogForgingDemo.cs.
    public static class LogForgingBaseline
    {
        public static void LogUnsanitized(ILogger logger, string[] args)
        {
            string raw = args.Length > 1 ? args[1] : Console.ReadLine() ?? string.Empty;
            logger.LogError("Failed login for user: " + raw);
        }
    }
}
