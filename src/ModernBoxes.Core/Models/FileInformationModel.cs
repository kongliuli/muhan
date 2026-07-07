using System;

namespace ModernBoxes.Core.Models
{
    public class FileInformationModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string CreateTime { get; set; } = string.Empty;
        public string ChangeTime { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
    }
}
