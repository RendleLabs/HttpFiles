using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace RendleLabs.HttpFiles.WebTests
{
    public class PutTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public PutTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PutsAndGetsTextFile()
        {
            try
            {
                var client = ClientHelper.CreateClient(_factory);

                var path = "/put/test/hello.txt?type=text";
                var request = new HttpRequestMessage(HttpMethod.Put, path);
                request.Content = new StringContent("Hello World!", Encoding.UTF8, "text/text");

                ClientHelper.SignRequest(request);

                var response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);

                request = new HttpRequestMessage(HttpMethod.Get, "/get/test/hello.txt");
                ClientHelper.SignRequest(request);
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

        [Fact]
        public async Task PutsAndGetsBinaryFile()
        {
            var starship = new Starship {Name = "Heart of Gold"};
            var content = MessagePackSerializer.Serialize(starship);
            try
            {
                var client = ClientHelper.CreateClient(_factory);

                var path = "/put/test/heartofgold.pack?type=binary";
                var byteContent = new ByteArrayContent(content);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/binary");
                var request = new HttpRequestMessage(HttpMethod.Put, path)
                {
                    Content = new ByteArrayContent(content)
                };

                ClientHelper.SignRequest(request);

                var response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);

                request = new HttpRequestMessage(HttpMethod.Get, "/get/test/heartofgold.pack");
                ClientHelper.SignRequest(request);
                response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("application/binary", response.Content.Headers.ContentType.MediaType);
                content = await response.Content.ReadAsByteArrayAsync();
                starship = MessagePackSerializer.Deserialize<Starship>(content);
                Assert.Equal("Heart of Gold", starship.Name);
            }
            finally
            {
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "test"), true);
            }
        }

        [MessagePackObject]
        public class Starship
        {
            [Key(0)]
            public string Name { get; set; }
        }
    }
}