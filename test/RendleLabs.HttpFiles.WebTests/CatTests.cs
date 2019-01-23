using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace RendleLabs.HttpFiles.WebTests
{
    public class CatTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;
        private static readonly byte[] Key = GenerateKey();

        public CatTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PutsAndGetsFile()
        {
            try
            {
                var client = _factory.WithWebHostBuilder(builder =>
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

                var path = "/put/test/hello.txt?type=text";
                var request = new HttpRequestMessage(HttpMethod.Put, path);
                request.Content = new StringContent("Hello World!", Encoding.UTF8, "text/text");
            
                SignRequest(request);
            
                var response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);

                request = new HttpRequestMessage(HttpMethod.Get, "/get/test/hello.txt");
                SignRequest(request);
                response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("text/text", response.Content.Headers.ContentType.MediaType);
                var text = await response.Content.ReadAsStringAsync();
                Assert.Equal("Hello World!", text);
            }
            finally
            {
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "test"), true);
            }
        }

        private static void SignRequest(HttpRequestMessage request)
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
