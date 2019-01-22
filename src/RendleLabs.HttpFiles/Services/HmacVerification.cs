using System;
using System.Buffers;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using RendleLabs.HttpFiles.Options;

namespace RendleLabs.HttpFiles.Services
{
    public interface IHmacVerification
    {
        bool Verify(ReadOnlySpan<char> timestamp, ReadOnlySpan<char> data, ReadOnlySpan<char> providedHash);
    }

    public class HmacVerification : IHmacVerification
    {
        private const int MaximumHashLength = 64;
        private readonly string _algorithmName;
        private readonly byte[] _key;
        private readonly int _maxTimestampMargin = 60;

        // ReSharper disable AssignNullToNotNullAttribute
        public HmacVerification(IOptions<SecurityOptions> options) : this(options.Value?.AlgorithmName, options.Value?.Key, options.Value?.MaxTimestampMargin)
        // ReSharper restore AssignNullToNotNullAttribute
        {
        }

        public HmacVerification([NotNull] string algorithmName, [NotNull] string key, int? maxTimestampMargin)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _algorithmName = algorithmName ?? throw new ArgumentNullException(nameof(algorithmName));
            _maxTimestampMargin = maxTimestampMargin ?? 60;
            _key = Convert.FromBase64String(key);
        }

        public bool Verify(ReadOnlySpan<char> timestamp, ReadOnlySpan<char> data, ReadOnlySpan<char> providedHash)
        {
            if (!CheckTimestamp(timestamp)) return false;

            using (var providedHashBytes = MemoryPool<byte>.Shared.Rent(MaximumHashLength))
            {
                if (!Convert.TryFromBase64Chars(providedHash, providedHashBytes.Memory.Span, out int providedHashByteCount))
                {
                    return false;
                }

                var providedHashSpan = providedHashBytes.Memory.Span.Slice(0, providedHashByteCount);

                using (var dataBytes = MemoryPool<byte>.Shared.Rent(data.Length * 4 + timestamp.Length))
                {
                    var dataSpan = CreatePlaintext(timestamp, data, dataBytes.Memory.Span);

                    return CompareHash(dataSpan, providedHashSpan);
                }
            }
        }

        private static Span<byte> CreatePlaintext(ReadOnlySpan<char> timestamp, ReadOnlySpan<char> data, Span<byte> target)
        {
            int timestampByteCount = Encoding.UTF8.GetBytes(timestamp, target);
            var dataSpan = target.Slice(timestampByteCount);
            int dataByteCount = Encoding.UTF8.GetBytes(data, dataSpan);
            return target.Slice(0, timestampByteCount + dataByteCount);
        }

        private bool CompareHash(Span<byte> dataSpan, Span<byte> providedHashSpan)
        {
            using (var computedHashBytes = MemoryPool<byte>.Shared.Rent(MaximumHashLength))
            {
                using (var hash = KeyedHashAlgorithm.Create(_algorithmName))
                {
                    hash.Key = _key;

                    var computedHash = computedHashBytes.Memory.Span;

                    hash.TryComputeHash(dataSpan, computedHash, out int computedHashByteCount);
                    
                    if (providedHashSpan.Length != computedHashByteCount)
                    {
                        return false;
                    }

                    computedHash = computedHash.Slice(0, computedHashByteCount);

                    return providedHashSpan.SequenceEqual(computedHash);
                }
            }
        }

        private bool CheckTimestamp(ReadOnlySpan<char> timestamp)
        {
            if (!DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var time))
            {
                return false;
            }

            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - time.ToUnixTimeSeconds()) > _maxTimestampMargin)
            {
                return false;
            }

            return true;
        }
    }
}