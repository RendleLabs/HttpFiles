using System;
using Microsoft.AspNetCore.Http;

namespace RendleLabs.HttpFiles.Extensions
{
    public static class QueryCollectionExtensions
    {
        public static bool TryGetEnum<T>(this IQueryCollection query, string key, out T value)
            where T : struct, Enum
        {
            if (query.TryGetValue(key, out var typeValues))
            {
                return Enum.TryParse<T>(typeValues[0], true, out value);
            }

            value = default;
            return false;
        }

        public static T GetEnumOrDefault<T>(this IQueryCollection query, string key, T defaultValue = default)
            where T : struct, Enum
        {
            return query.TryGetEnum(key, out T value) ? value : defaultValue;
        }

        public static string GetStringOrDefault(this IQueryCollection query, string key, string defaultValue = default)
        {
            if (query.TryGetValue(key, out var typeValues) && typeValues.Count == 1)
            {
                return typeValues[0];
            }

            return defaultValue;
        }
    }
}