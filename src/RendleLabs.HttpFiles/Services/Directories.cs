using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using RendleLabs.HttpFiles.Options;

namespace RendleLabs.HttpFiles.Services
{
    public interface IDirectories
    {
        IEnumerable<string> ListFiles(string directory, string pattern = null);
    }

    public class Directories : IDirectories
    {
        private readonly StorageOptions _storageOptions;
        private readonly string _baseDirectory;

        public Directories(IOptions<StorageOptions> storageOptions)
        {
            _storageOptions = storageOptions.Value;
            _baseDirectory = string.IsNullOrWhiteSpace(_storageOptions.BaseDirectory) ? Path.GetTempPath() : _storageOptions.BaseDirectory;
        }

        public IEnumerable<string> ListFiles(string directory, string pattern = null)
        {
            var fullPath = Path.Combine(_baseDirectory, directory);

            var files = Directory.EnumerateFiles(fullPath, pattern ?? "*", SearchOption.AllDirectories);

            files = files.Select(f => Path.Combine(directory, Path.GetFileName(f)));

            if (Path.PathSeparator != '/')
            {
                files = files.Select(f => f.Replace(Path.PathSeparator, '/'));
            }

            return files;
        }
    }
}