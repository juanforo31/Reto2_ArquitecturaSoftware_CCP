using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class HmacHelper
{

    public string GenerarHmac<T>(T payload)
    {
        var stopwatch = Stopwatch.StartNew();

        string secretKey = "Arquitectura de software";

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        Console.WriteLine($"{{json}}");

        var json1 = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        //Console.WriteLine($"keyBytes = {BitConverter.ToString(keyBytes)}");
        // var msgBytes = Encoding.UTF8.GetBytes(json);
        var msgBytes = Encoding.UTF8.GetBytes(json1);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(msgBytes);
        // TODO: Guardar tiempo hash creado
        stopwatch.Stop();
        Console.WriteLine($"Tiempo creación firma datos entrada: {stopwatch.ElapsedMilliseconds} ms");

        return Convert.ToHexString(hash).ToLower();
    }
}
