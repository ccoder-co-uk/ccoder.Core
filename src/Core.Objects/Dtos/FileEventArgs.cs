using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Core.Objects.Dtos
{
    public class FileEventArgs
    {
        [Key]
        public int DirectoryId { get; set; }

        [Required]
        public string DirectoryConfigurationName { get; set; }

        [Required]
        public string UNCPath { get; set; }

        [Required]
        public string FileNameFilter { get; set; }

        [Required]
        public string FullPath { get; set; }

        [Required]
        public string FileName { get; set; }

        public WatcherChangeTypes EventType { get; set; }

        public virtual IEnumerable<DataItem> AdditionalData { get; set; }

        public FileEventArgs() { }

        public FileEventArgs(FileSystemEventArgs e)
        {
            EventType = e.ChangeType;
            FileName = e.Name;
            FullPath = e.FullPath;
        }
    }
}
