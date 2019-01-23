using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RendleLabs.HttpFiles.Models;
using RendleLabs.HttpFiles.Options;

namespace RendleLabs.HttpFiles.Services
{
    public interface IDirectories
    {
        Task<FileList> ListFilesAsync(string directory, string pattern = null);
    }

    public class Directories : IDirectories
    {
        private readonly string _baseDirectory;

        public Directories(IOptions<StorageOptions> storageOptions)
        {
            _baseDirectory = string.IsNullOrWhiteSpace(storageOptions.Value.BaseDirectory) ? Path.GetTempPath() : storageOptions.Value.BaseDirectory;
        }

        public async Task<FileList> ListFilesAsync(string directory, string pattern = null)
        {
            var fullPath = Path.Combine(_baseDirectory, directory);

            if (!Directory.Exists(fullPath))
            {
                return null;
            }

            var files = Directory.EnumerateFiles(fullPath, pattern ?? "*", SearchOption.AllDirectories);

            files = files.Select(f => Path.Combine(directory, Path.GetFileName(f)));

            if (Path.PathSeparator != '/')
            {
                files = files.Select(f => f.Replace(Path.PathSeparator, '/'));
            }

            var items = await Task.WhenAll(files.Select(f => CreateFileListItemAsync(f, directory)));

            return new FileList
            {
                Files = items
            };
        }

        private static async Task<FileListItem> CreateFileListItemAsync(string file, string virtualDirectory)
        {
            var virtualFile = Path.Combine(virtualDirectory, Path.GetFileName(file)).Replace(Path.PathSeparator, '/');
            var path = $"{file}.metadata";
            if (File.Exists(path))
            {
                using (var reader = File.OpenText(path))
                {
                    var json = await reader.ReadToEndAsync();
                    var metadata = JsonConvert.DeserializeObject<FileMetadata>(json);
                    return new FileListItem
                    {
                        File = virtualFile,
                        Type = metadata.Type == FileType.Text ? "Text" : "Binary"
                    };
                }
            }

            return new FileListItem
            {
                File = virtualFile,
                Type = "Binary"
            };
        }
    }
}