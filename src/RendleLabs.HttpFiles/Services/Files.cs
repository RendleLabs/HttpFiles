using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RendleLabs.HttpFiles.Models;
using RendleLabs.HttpFiles.Options;

namespace RendleLabs.HttpFiles.Services
{
    public interface IFiles
    {
        ValueTask<string> ReadAsync(string file);
        Task WriteAsync(string file, Stream requestBody, FileType type);
        bool GetFilePath(string file, out string path);
        Task<FileSource> TryGetFileSource(string file);
    }

    public class Files : IFiles
    {
        private readonly string _baseDirectory;

        public Files(IOptions<StorageOptions> storageOptions)
        {
            var options = storageOptions.Value;
            _baseDirectory = string.IsNullOrWhiteSpace(options.BaseDirectory) ? Path.GetTempPath() : options.BaseDirectory;
        }

        public bool GetFilePath(string file, out string path)
        {
            path = Path.Combine(_baseDirectory, file);
            if (File.Exists(path)) return true;
            path = default;
            return false;
        }

        public async Task<FileSource> TryGetFileSource(string file)
        {
            var path = Path.Combine(_baseDirectory, file);
            if (!File.Exists(path))
            {
                return default;
            }

            var source = new FileSource
            {
                FullPath = path
            };
            
            var metadataPath = $"{path}._metadata";
            if (File.Exists(metadataPath))
            {
                using (var reader = File.OpenText(metadataPath))
                {
                    var json = await reader.ReadToEndAsync();
                    var metadata = JsonConvert.DeserializeObject<FileMetadata>(json);
                    source.Type = metadata.Type;
                }
            }

            return source;
        }

        public ValueTask<string> ReadAsync(string file)
        {
            async Task<string> ReadAsync(string path)
            {
                using (var reader = File.OpenText(path))
                {
                    return await reader.ReadToEndAsync();
                }
            }

            var fullPath = Path.Combine(_baseDirectory, file);
            return File.Exists(fullPath) ? new ValueTask<string>(ReadAsync(fullPath)) : new ValueTask<string>((string) null);
        }

        public async Task WriteAsync(string file, Stream requestBody, FileType fileType)
        {
            var directory = Path.Combine(_baseDirectory, Path.GetDirectoryName(file));
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = Path.GetFileName(file);
            
            var filePath = Path.Combine(directory, fileName);

            using (var target = File.Create(file))
            {
                await requestBody.CopyToAsync(target);
            }

            var metadata = new FileMetadata {Type = fileType};
            using (var metadataWriter = File.CreateText($"{filePath}._metadata"))
            {
                await metadataWriter.WriteAsync(JsonConvert.SerializeObject(metadata));
            }
        }
    }

    public enum FileType
    {
        Binary,
        Text,
    }
}