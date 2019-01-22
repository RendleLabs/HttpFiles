using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RendleLabs.HttpFiles.Options;

namespace RendleLabs.HttpFiles.Services
{
    public interface IFiles
    {
        ValueTask<string> ReadAsync(string file);
        Task WriteTextAsync(string file, Stream requestBody);
    }

    public class Files : IFiles
    {
        private readonly string _baseDirectory;

        public Files(IOptions<StorageOptions> storageOptions)
        {
            var options = storageOptions.Value;
            _baseDirectory = string.IsNullOrWhiteSpace(options.BaseDirectory) ? Path.GetTempPath() : options.BaseDirectory;
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

        public async Task WriteTextAsync(string file, Stream requestBody)
        {
            var directory = Path.GetDirectoryName(file);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var target = File.Create(file))
            {
                await requestBody.CopyToAsync(target);
            }
        }
    }
}