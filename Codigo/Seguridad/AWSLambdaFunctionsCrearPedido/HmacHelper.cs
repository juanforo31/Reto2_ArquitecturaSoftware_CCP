using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambdaFunctionsCrearPedido
{

    internal class HmacHelper
    {
        private readonly string secretKey = "Arquitectura de software";

        public HmacHelper()
        {
        }

        public string GenerarHmac<T>(T payload)
        {
            var stopwatch = Stopwatch.StartNew();

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = false
            });
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var msgBytes = Encoding.UTF8.GetBytes(json);

            using var hmac = new HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(msgBytes);
            stopwatch.Stop();
            Console.WriteLine($"Tiempo creación firma datos entrada: {stopwatch.ElapsedMilliseconds} ms");

            return Convert.ToHexString(hash).ToLower();
        }
    }
}
