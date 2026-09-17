using System.Security.Cryptography;
using System.Text;
using Alpha.Lib;
using Microsoft.Data.SqlClient;

namespace Alpha.App;

// WARNING: This project is intentionally insecure. It exists only to exercise
// Snyk Code (SAST) and Snyk Open Source (SCA). Do not copy any of this code.
public static class Program
{
    // Snyk Code: Use of Hardcoded Credentials (CWE-798) — literal password in a connection string.
    private const string ConnectionString =
        "Server=db.internal;Database=Orders;User Id=sa;Password=P@ssw0rd123!;";

    // Snyk Code: Hardcoded Secret (CWE-547).
    private const string ApiKey = "AKIAIOSFODNN7EXAMPLE";

    public static void Main(string[] args)
    {
        string userId = args.Length > 0 ? args[0] : Console.ReadLine() ?? string.Empty;

        LookupUser(userId);

        Console.WriteLine(HashPassword(userId));
        Console.WriteLine($"Authenticating with key {ApiKey}");
        Console.WriteLine(OrderRepository.Describe());
    }

    // Snyk Code: SQL Injection (CWE-89) — untrusted input concatenated into a query.
    private static void LookupUser(string userId)
    {
        using var connection = new SqlConnection(ConnectionString);
        var query = "SELECT * FROM Users WHERE Id = '" + userId + "'";
        using var command = new SqlCommand(query, connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            Console.WriteLine(reader[0]);
        }
    }

    // Snyk Code: Use of a Broken or Risky Cryptographic Algorithm (CWE-327) — MD5.
    private static string HashPassword(string password)
    {
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hash);
    }
}
