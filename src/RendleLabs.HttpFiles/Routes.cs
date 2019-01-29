using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using RendleLabs.AspNetCore.RoutingWithServices;
using RendleLabs.HttpFiles.Extensions;
using RendleLabs.HttpFiles.Models;
using RendleLabs.HttpFiles.Services;

namespace RendleLabs.HttpFiles
{
    public static class Routes
    {
        public static void Configure(IRouteBuilder builder)
        {
            builder.MapGet<IFiles>("/get/{*file}", GetFile);
            builder.MapPut<IFiles>("/put/{*file}", PutFile);
            builder.MapDelete<IFiles>("/delete/{*file}", DeleteFile);
        }

        private static async Task GetFile(HttpRequest request, HttpResponse response, RouteData routeData, IFiles files)
        {
            var file = routeData.GetString("file");
            var source = await files.TryGetFileSource(file);
            if (source == null)
            {
                response.StatusCode = 404;
                return;
            }

            response.StatusCode = 200;
            if (source.Type == FileType.Text)
            {
                response.ContentType = "text/text";
                await response.SendFileAsync(source.FullPath);
            }
            else
            {
                response.ContentType = "application/binary";
                using (var stream = File.OpenRead(source.FullPath))
                {
                    await stream.CopyToAsync(response.Body);
                }
            }
        }

        private static async Task PutFile(HttpRequest request, HttpResponse response, RouteData routeData, IFiles files)
        {
            var file = routeData.GetString("file");
            var type = request.Query.GetEnumOrDefault("type", FileType.Binary);

            await files.WriteAsync(file, request.Body, type);
            response.StatusCode = 201;
        }

        private static Task DeleteFile(HttpRequest request, HttpResponse response, RouteData routeData, IFiles files)
        {
            var file = routeData.GetString("file");

            response.StatusCode = files.Delete(file) ? 200 : 404;

            return Task.CompletedTask;
        }
    }
}