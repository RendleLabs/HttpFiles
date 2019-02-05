using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using RendleLabs.HttpFiles.Services;
using Xunit;
using Xunit.Abstractions;

namespace RendleLabs.HttpFiles.UnitTests
{
    public class HmacVerificationTests
    {
        private readonly ITestOutputHelper _output;
        private const string AlgorithmName = "HMACSHA256";

        public HmacVerificationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ReturnsTrueForValidHash()
        {
            var now = DateTimeOffset.UtcNow;

            var path = "/ls";

            var plaintext = $"{now:O}{path}";

            var key = new byte[64];

            System.Security.Cryptography.RandomNumberGenerator.Create().GetNonZeroBytes(key);

            var hmac = new HMACSHA256(key);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            
            var target = new HmacVerification(Convert.ToBase64String(key), 60);
            
            Assert.True(target.Verify(now.ToString("O"), path, Convert.ToBase64String(hash)));
        }

        [Fact]
        public void ReturnsFalseForInvalidHash()
        {
            var now = DateTimeOffset.UtcNow;

            var path = "/ls";

            var plaintext = $"{now:O}{path}";

            var key = new byte[64];

            RandomNumberGenerator.Create().GetNonZeroBytes(key);

            var hmac = new HMACSHA256(key);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext));

            hash[0] = (byte) (hash[0] > 0 ? hash[0] - (byte)1 : (byte)255);
            
            var target = new HmacVerification(Convert.ToBase64String(key), 60);
            
            Assert.False(target.Verify(now.ToString("O"), path, Convert.ToBase64String(hash)));
        }

        [Theory]
        [MemberData(nameof(InvalidTimestamps))]
        public void ReturnsFalseForOutOfDateTimestamp(DateTimeOffset now)
        {
            var path = "/ls";

            var plaintext = $"{now:O}{path}";

            var key = new byte[64];

            System.Security.Cryptography.RandomNumberGenerator.Create().GetNonZeroBytes(key);

            var hmac = new HMACSHA256(key);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext));

            var target = new HmacVerification(Convert.ToBase64String(key), 60);
            
            Assert.False(target.Verify(now.ToString("O"), path, Convert.ToBase64String(hash)));
        }

        [Theory]
        [MemberData(nameof(CloseEnoughTimestamps))]
//        [Fact]
        public void ReturnsTrueForCloseEnoughTimestamps(DateTimeOffset now)
        {
//            var now = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(1);
            _output.WriteLine($"{now:O} - {DateTimeOffset.UtcNow:O}");
            var path = "/ls";

            var plaintext = $"{now:O}{path}";

            var key = new byte[64];

            RandomNumberGenerator.Create().GetNonZeroBytes(key);

            var hmac = new HMACSHA256(key);

            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(plaintext));

            var target = new HmacVerification(Convert.ToBase64String(key), 60);
            
            Assert.True(target.Verify(now.ToString("O"), path, Convert.ToBase64String(hash)));
        }

        public static IEnumerable<object[]> InvalidTimestamps()
        {
            var now = DateTimeOffset.UtcNow;
            yield return new object[] {now - TimeSpan.FromSeconds(61)};
            yield return new object[] {now - TimeSpan.FromDays(1)};
            yield return new object[] {now + TimeSpan.FromSeconds(120)};
            yield return new object[] {now + TimeSpan.FromDays(1)};
        }
        
        public static IEnumerable<object[]> CloseEnoughTimestamps()
        {
            for (int seconds = 1; seconds < 60; seconds *= 2)
            {
                var now = DateTimeOffset.UtcNow;
                Console.WriteLine(now);
                
                yield return new object[] {now - TimeSpan.FromSeconds(seconds)};
                yield return new object[] {now + TimeSpan.FromSeconds(seconds)};
            }
        }
    }
}
