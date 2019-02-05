using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using RendleLabs.HttpFiles.Options;

namespace RendleLabs.HttpFiles.Services
{
    public class HmacVerification : IHmacVerification
    {
        private const int MaximumHashLength = 64;
        private readonly byte[] _key;
        private readonly int _maxTimestampMargin;

        // ReSharper disable AssignNullToNotNullAttribute
        public HmacVerification(IOptions<SecurityOptions> options) : this(options.Value?.Key, options.Value?.MaxTimestampMargin)
        // ReSharper restore AssignNullToNotNullAttribute
        {
        }

        public HmacVerification(string key, int? maxTimestampMargin)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _maxTimestampMargin = maxTimestampMargin ?? 60;
            _key = Convert.FromBase64String(key);
        }

        public bool Verify(string timestamp, string data, string providedHash)
        {
            if (!CheckTimestamp(timestamp)) return false;

            var providedHashBytes = Convert.FromBase64String(providedHash);

            using (var dataBytes = MemoryPool<byte>.Shared.Rent(data.Length * 4 + timestamp.Length))
            {
                var dataSpan = CreatePlaintext(timestamp, data, dataBytes.Memory.Span);

                return CompareHash(dataSpan, providedHashBytes);
            }
        }

        private static Span<byte> CreatePlaintext(string timestamp, string data, Span<byte> target)
        {
            var timestampBytes = Encoding.UTF8.GetBytes(timestamp);
            var s = target;
            timestampBytes.AsSpan().CopyTo(s);
            s = s.Slice(timestampBytes.Length);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            dataBytes.AsSpan().CopyTo(s);
            return target.Slice(0, timestampBytes.Length + dataBytes.Length);
        }

        private bool CompareHash(Span<byte> dataSpan, Span<byte> providedHashSpan)
        {
            using (var hash = new HMACSHA256(_key))
            {
                var computedHash = hash.ComputeHash(dataSpan.ToArray());

                if (providedHashSpan.Length != computedHash.Length)
                {
                    return false;
                }

                return providedHashSpan.SequenceEqual(computedHash);
            }
        }

        private bool CheckTimestamp(string timestamp)
        {
            if (!DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var time))
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow;
            
            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - time.ToUnixTimeSeconds()) > _maxTimestampMargin)
            {
                return false;
            }

            return true;
        }
    }
}