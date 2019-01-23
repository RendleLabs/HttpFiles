namespace RendleLabs.HttpFiles.Models
{
    public class FileList
    {
        public FileListItem[] Files { get; set; }
    }

    public class FileListItem
    {
        public string File { get; set; }
        public string Type { get; set; }
    }
}