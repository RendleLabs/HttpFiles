using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RendleLabs.HttpFiles.WebTests
{
    internal static class ClientHelper
    {
        private static readonly byte[] Key = GenerateKey();
        
        internal static HttpClient CreateClient(WebApplicationFactory<Startup> factory)
        {
            return factory.WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration(config =>
                    {
                        config.AddInMemoryCollection(new[]
                        {
                            new KeyValuePair<string, string>("Storage:BaseDirectory", Environment.CurrentDirectory),
                            new KeyValuePair<string, string>("Security:AlgorithmName", "HMACSHA256"),
                            new KeyValuePair<string, string>("Security:Key", Convert.ToBase64String(Key)),
                        });
                    });
                })
                .CreateClient();
        }
        
        internal static void SignRequest(HttpRequestMessage request)
        {
            var timestamp = DateTimeOffset.UtcNow.ToString("O");
            request.Headers.Add("x-timestamp", timestamp);
            var plainText = $"{timestamp}{request.RequestUri}";
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            using (var hash = KeyedHashAlgorithm.Create("HMACSHA256"))
            {
                hash.Key = Key;

                var hashBytes = hash.ComputeHash(plainBytes);
                request.Headers.Authorization = new AuthenticationHeaderValue("HMAC", Convert.ToBase64String(hashBytes));
            }
        }
        
        private static byte[] GenerateKey()
        {
            var key = new byte[64];
            RandomNumberGenerator.Create().GetNonZeroBytes(key);
            return key;
        }
    }
}