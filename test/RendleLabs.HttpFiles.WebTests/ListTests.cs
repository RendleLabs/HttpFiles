using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using Xunit;

namespace RendleLabs.HttpFiles.WebTests
{
    public class ListTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public ListTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task PutsAndListsFiles()
        {
            try
            {
                var client = ClientHelper.CreateClient(_factory);

                // Create a file
                var path = "/put/test/subject.client.pack?type=text";
                var request = new HttpRequestMessage(HttpMethod.Put, path);
                request.Content = new StringContent("Hello World!", Encoding.UTF8, "text/text");
                ClientHelper.SignRequest(request);
                var response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);

                path = "/ls/test?pattern=subject.*.pack";

                request = new HttpRequestMessage(HttpMethod.Get, path);
                ClientHelper.SignRequest(request);
                response = await client.SendAsync(request);
                Assert.True(response.IsSuccessStatusCode);
                Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
                var text = await response.Content.ReadAsStringAsync();
                var jobj = JObject.Parse(text);
                Assert.True(jobj.ContainsKey("files"));
                if (jobj["files"] is JArray list)
                {
                    foreach (var token in list)
                    {
                        if (token is JObject file)
                        {
                            Assert.Equal("test/subject.client.pack", token["file"].ToString());
                        }
                    }
                }
                else
                {
                    Assert.True(jobj["files"] is JArray);
                }
            }
            finally
            {
                Directory.Delete(Path.Combine(Environment.CurrentDirectory, "test"), true);
            }
        }
    }
}