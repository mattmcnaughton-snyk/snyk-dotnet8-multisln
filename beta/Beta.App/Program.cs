using System.Diagnostics;
using System.Xml;
using Beta.Lib;
using log4net;

namespace Beta.App;

// WARNING: This project is intentionally insecure. It exists only to exercise
// Snyk Code (SAST) and Snyk Open Source (SCA). Do not copy any of this code.
public static class Program
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

    public static void Main(string[] args)
    {
        string input = args.Length > 0 ? args[0] : Console.ReadLine() ?? string.Empty;
        Log.Info($"processing request: {input}");

        RunReport(input);
        Console.WriteLine(ReadConfig(input));
        ParseDocument(input);
        Console.WriteLine(ApiClient.Ping(input));
    }

    // Snyk Code: OS Command Injection (CWE-78) — untrusted input concatenated into a shell command.
    private static void RunReport(string name)
    {
        var process = new Process();
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = "-c \"generate-report " + name + "\"";
        process.Start();
    }

    // Snyk Code: Path Traversal (CWE-23) — untrusted input concatenated into a file path.
    private static string ReadConfig(string fileName)
    {
        var path = "/etc/app/config/" + fileName;
        return File.ReadAllText(path);
    }

    // Snyk Code: XML External Entity (XXE) Injection (CWE-611) — DTD + external resolver enabled.
    private static void ParseDocument(string xml)
    {
        var doc = new XmlDocument
        {
            XmlResolver = new XmlUrlResolver(),
        };
        doc.LoadXml(xml);
        Console.WriteLine(doc.DocumentElement?.Name);
    }
}
