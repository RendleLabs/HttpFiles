using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace RendleLabs.HttpFiles.WebTests
{
    public class DeleteTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public DeleteTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PutsAndGetsAndDeletesFile()
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
                
                request = new HttpRequestMessage(HttpMethod.Delete, "/delete/test/hello.txt");
                ClientHelper.SignRequest(request);
                response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);

                request = new HttpRequestMessage(HttpMethod.Get, "/get/test/hello.txt");
                ClientHelper.SignRequest(request);
                response = await client.SendAsync(request);
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            }
            finally
            {
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "test"), true);
            }
        }
    }
}