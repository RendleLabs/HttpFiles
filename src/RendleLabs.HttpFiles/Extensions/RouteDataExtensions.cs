using Microsoft.AspNetCore.Routing;

namespace RendleLabs.HttpFiles.Extensions
{
    public static class RouteDataExtensions
    {
        public static string GetString(this RouteData routeData, string key)
        {
            return routeData.Values.TryGetValue(key, out var obj) ? obj?.ToString() : default;
        }
    }
}