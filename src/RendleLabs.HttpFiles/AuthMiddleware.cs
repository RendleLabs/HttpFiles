using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using RendleLabs.HttpFiles.Services;

namespace RendleLabs.HttpFiles
{
    public sealed class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHmacVerification _hmacVerification;

        public AuthMiddleware(RequestDelegate next, IHmacVerification hmacVerification)
        {
            _next = next;
            _hmacVerification = hmacVerification;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (!TryGetHeaders(context.Request.Headers, out var headers))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            var hash = headers.authorization;

            var path = $"{context.Request.Path}{context.Request.QueryString}";
            if (!_hmacVerification.Verify(headers.timestamp, path, hash))
            {
                context.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            return _next(context);
        }

        private static bool TryGetHeaders(IHeaderDictionary headers, out (string timestamp, string authorization) values)
        {
            if (!headers.TryGetValue("x-timestamp", out var timestampHeader) || timestampHeader.Count != 1)
            {
                values = default;
                return false;
            }
            
            if (!headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader) || authorizationHeader.Count != 1 || !authorizationHeader[0].StartsWith("HMAC "))
            {
                values = default;
                return false;
            }

            values = (timestampHeader[0], authorizationHeader[0]);
            return true;
        }
    }
}