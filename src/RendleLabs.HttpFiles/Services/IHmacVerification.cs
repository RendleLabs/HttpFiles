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
    public interface IHmacVerification
    {
        bool Verify(string timestamp, string data, string providedHash);
    }
}